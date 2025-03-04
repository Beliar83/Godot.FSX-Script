#if TOOLS
#nullable enable
using System.Xml;
using Godot;
using CodeEdit = Godot.CodeEdit;
using System;
using System.Reflection;
#if FSX_SERVER_ACTIVE
using System.IO;
using Godot.FSharp;
#endif
namespace FsxScriptAddon;

[Tool]
[GlobalClass]
public partial class FsxScriptPlugin : EditorPlugin
{
    private const string FsxScriptSdkName = "Godot.FsxScript.Sdk";
    private const string FsxScriptSdkVersion = "0.9.0";
    private const string SdkTag = "Sdk";
    private const string NameAttribute = "Name";
    private const string VersionAttribute = "Version";
#if FSX_SERVER_ACTIVE    
    private const string ItemGroupTag = "ItemGroup";
    private const string FSXNodesName = "FsxNodes";
    private const string LabelAttribute = "Label";
    private const string ReferenceTag = "Reference";
    private const string ConditionAttribute = "Condition";
    private const string IncludeAttribute = "Include";
#endif
    private const string GodotSharpPathProperty = "GodotSharpPath";
    private const string PropertyGroupTag = "PropertyGroup";
    private const string GeneratedPath = "Generated";
    private bool sdkWasAdded;
#if FSX_SERVER_ACTIVE
    private FsxScriptExportPlugin? exportPlugin;
    internal static readonly Godot.Collections.Dictionary<string, FsxScriptSession> Sessions = new();
#endif
#if FSX_SERVER_ACTIVE
    private static bool IsInitialized()
    {
        return true;
    }
#else
    private static bool IsInitialized() => false;
#endif
    public override void _EnterTree()
    {
#if FSX_SERVER_ACTIVE
        System.Environment.SetEnvironmentVariable("FSHARP_COMPILER_BIN", AppDomain.CurrentDomain.BaseDirectory);
        if (SetupFsxScript())
        {
            AcceptDialog dialog = new();
            dialog.DialogAutowrap = true;
            dialog.DialogText =
                "FsxScript values were updated. Dotnet project should be rebuild.";
            AddChild(dialog);

            dialog.PopupCentered();
        }

        Initialize();
#else
        AcceptDialog installSdkDialog = new();
        installSdkDialog.DialogAutowrap = true;
        installSdkDialog.DialogText =
            "FsxScript SDK will be added to the dotnet project. The project then needs to be rebuild and reloaded.";
        AddChild(installSdkDialog);
        
        installSdkDialog.Confirmed += () =>
        {
            sdkWasAdded = SetupFsxScript();
        };
        
        installSdkDialog.PopupCentered();
#endif
    }

#if FSX_SERVER_ACTIVE
    private void Initialize()
    {
        foreach (Script script in EditorInterface.Singleton.GetScriptEditor().GetOpenScripts())
        {
            if (!script.IsClass("FsxScript"))
            {
                continue;
            }

            GetOrCreateSession(script.GetPath());
        }

        ScriptSession.SetBasePath(ProjectSettings.GlobalizePath("res://"));
        EditorInterface.Singleton.GetScriptEditor().EditorScriptChanged += OnEditorScriptChanged;
        EditorInterface.Singleton.GetScriptEditor().ScriptClose += OnScriptClose;
        exportPlugin = new FsxScriptExportPlugin();
        AddExportPlugin(exportPlugin);
    }
#endif

    private static void OnEditorScriptChanged(Script? script)
    {
        if (!script?.IsClass("FsxScript") ?? true)
        {
            return;
        }

        (EditorInterface.Singleton.GetScriptEditor().GetCurrentEditor().GetBaseEditor() as CodeEdit)!.IndentUseSpaces =
            true;
    }

    private static void OnScriptClose(Script script)
    {
#if FSX_SERVER_ACTIVE
        if (!script.IsClass("FsxScript"))
        {
            return;
        }

        string path = script.GetPath();
        Sessions.Remove(path);
#endif
    }

    /// <inheritdoc />
    public override void _Process(double _)
    {
#if FSX_SERVER_ACTIVE
        if (sdkWasAdded)
        {
            Initialize();
            sdkWasAdded = false;
        }

        foreach (Script script in EditorInterface.Singleton.GetScriptEditor().GetOpenScripts())
        {
            if (!script.IsClass("FsxScript"))
            {
                continue;
            }

            string path = script.GetPath();
            if (Sessions.TryGetValue(path, out FsxScriptSession? session))
            {
                continue;
            }

            GetOrCreateSession(path);
        }
#endif
    }

    public override void _ExitTree()
    {
#if FSX_SERVER_ACTIVE
        if (exportPlugin is not null)
        {
            RemoveExportPlugin(exportPlugin);
            exportPlugin = null;
        }

        EditorInterface.Singleton.GetScriptEditor().EditorScriptChanged -= OnEditorScriptChanged;
        EditorInterface.Singleton.GetScriptEditor().ScriptClose -= OnScriptClose;

        Sessions.Clear();
#endif
    }

#if FSX_SERVER_ACTIVE
    private static FsxScriptSession GetOrCreateSession(string path)
    {
        if (Sessions.TryGetValue(path, out FsxScriptSession? session))
        {
            return session;
        }

        session = new FsxScriptSession();
        session.ScriptPath = path;
        Sessions[path] = session;

        if (path == "GeneralFsxScriptSession")
        {
            return session;
        }

        string fullGeneratedPath = Path.Join(ProjectSettings.GlobalizePath("res://"), GeneratedPath);
        if (!Directory.Exists(fullGeneratedPath))
        {
            Directory.CreateDirectory(fullGeneratedPath);
        }
        
        string csFilePath = Path.Join(fullGeneratedPath, Path.ChangeExtension(Path.GetFileName(path), "cs"));
        if (!File.Exists(csFilePath))
        {
            File.WriteAllText(csFilePath, "//This file was generated by FsxScript as a placeholder");
        }
        
        session.UpdateScript();

        string assemblyName = ProjectSettings.GetSetting("dotnet/project/assembly_name").ToString();
        XmlDocument document = new();
        document.Load(ProjectSettings.GlobalizePath($"res://{assemblyName}.csproj"));
        if (document.DocumentElement is not null)
        {
            XmlNodeList itemGroups = document.DocumentElement.GetElementsByTagName(ItemGroupTag);
            XmlElement? fsxNodesElement = null;
            for (int i = 0; i < itemGroups.Count; i++)
            {
                XmlElement? element = itemGroups[i]! as XmlElement;
                if (element?.GetAttribute(LabelAttribute) == FSXNodesName)
                {
                    fsxNodesElement = element;
                    break;
                }
            }

            if (fsxNodesElement is null)
            {
                fsxNodesElement = document.CreateElement(ItemGroupTag);
                fsxNodesElement.SetAttribute(LabelAttribute, FSXNodesName);
                fsxNodesElement.SetAttribute(ConditionAttribute, "'$(Configuration)'!='DEBUG'");
                document.DocumentElement.AppendChild(fsxNodesElement);
            }

            XmlElement? nodeRefElement = null;

            string assemblyPath = Path.GetRelativePath(ProjectSettings.GlobalizePath("res://"),
                Path.ChangeExtension(ProjectSettings.GlobalizePath(path), "dll"));

            for (int i = 0; i < fsxNodesElement.ChildNodes.Count; i++)
            {
                XmlElement? element = fsxNodesElement.ChildNodes[i]! as XmlElement;
                if (element?.Name != ReferenceTag)
                {
                    continue;
                }

                if (element.GetAttribute(IncludeAttribute) == assemblyPath)
                {
                    nodeRefElement = element;
                }
            }

            if (nodeRefElement is null)
            {
                nodeRefElement = document.CreateElement(ReferenceTag);
                nodeRefElement.SetAttribute(IncludeAttribute, assemblyPath);
                fsxNodesElement.AppendChild(nodeRefElement);
            }

            document.Save(ProjectSettings.GlobalizePath($"res://{assemblyName}.csproj"));
        }

        return session;
    }
#endif
    private bool SetupFsxScript()
    {
        string assemblyName = ProjectSettings.GetSetting("dotnet/project/assembly_name").ToString();
        XmlDocument document = new();
        document.Load(ProjectSettings.GlobalizePath($"res://{assemblyName}.csproj"));


        if (document.DocumentElement is not null)
        {
            XmlNodeList propertyGroups = document.GetElementsByTagName(PropertyGroupTag);

#if FSX_SERVER_ACTIVE
            Version targetFramework = new(8, 0, 0);

            foreach (XmlElement propertyGroup in propertyGroups)
            {
                XmlNodeList targetFrameworksAttributes = propertyGroup.GetElementsByTagName("TargetFramework");
                if (targetFrameworksAttributes.Count > 0)
                {
                    string framework = targetFrameworksAttributes[^1]!.InnerText;
                    int firstDot = framework.Find('.');
                    string frameworkType = framework[..(firstDot - 1)];
                    if (!frameworkType.Equals("net", StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    string frameworkVersion = framework[(firstDot - 1)..];
                    while (frameworkVersion.Count(".") < 2)
                    {
                        frameworkVersion = $"{frameworkVersion}.0";
                    }

                    targetFramework = Version.Parse(frameworkVersion);
                }
            }


            DirectoryInfo directoryInfo =
                new(ProjectSettings.GlobalizePath($"{ScriptSession.FsxScriptPath}"));
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            FileInfo fileInfo = new(Path.Join(directoryInfo.FullName, "global.json"));

            {
                using FileStream fileStream = fileInfo.Exists ? fileInfo.OpenWrite() : fileInfo.Create();
                using StreamWriter streamWriter = new(fileStream);
                streamWriter.Write($$"""
                                     {
                                       "sdk": {
                                         "version": "{{targetFramework.ToString(3)}}",
                                         "rollForward": "latestMinor"
                                       }
                                     }
                                     """);
            }

#endif

            XmlElement? sdkElement = null;
            XmlNodeList sdkElements = document.DocumentElement.GetElementsByTagName(SdkTag);
            for (int i = 0; i < sdkElements.Count; i++)
            {
                XmlElement? element = sdkElements[i]! as XmlElement;
                if (element?.Attributes is null)
                {
                    continue;
                }

                if (element.GetAttribute(NameAttribute) == FsxScriptSdkName)
                {
                    sdkElement = element;
                    break;
                }
            }

            if (sdkElement is null)
            {
                sdkElement = document.CreateElement(SdkTag);
                sdkElement.SetAttribute(NameAttribute, FsxScriptSdkName);
                document.DocumentElement.InsertBefore(sdkElement, document.DocumentElement.FirstChild);
            }

            bool changed = sdkElement.GetAttribute(VersionAttribute) != FsxScriptSdkVersion;

            sdkElement.SetAttribute(VersionAttribute, FsxScriptSdkVersion);


            Assembly assembly = Assembly.Load("GodotSharp");

            changed = UpdateProperty(document, GodotSharpPathProperty, assembly.Location) | changed;
            changed = UpdateProperty(document, "FSharpCompilerPath", AppDomain.CurrentDomain.BaseDirectory) | changed;
            changed = UpdateProperty(document, "GeneratedPath", GeneratedPath) | changed;
            
            //FSharpCompilerPath
            if (changed)
            {
                document.Save(ProjectSettings.GlobalizePath($"res://{assemblyName}.csproj"));
            }

            return changed;
        }

        GD.PrintErr("Could not load dotnet project file");
        return false;
    }

    private static bool UpdateProperty(XmlDocument document, string propertyName, string propertyValue)
    {
        XmlNodeList propertyGroups = document.GetElementsByTagName(PropertyGroupTag);

        XmlElement? propertyElement = null;

        bool propertyChanged = false;
        foreach (XmlElement propertyGroup in propertyGroups)
        {
            XmlNodeList? matchingProperties = propertyGroup?.GetElementsByTagName(propertyName);
            if (!(matchingProperties?.Count > 0))
            {
                continue;
            }

            propertyElement = matchingProperties[0] as XmlElement;
            if (propertyElement is not null)
            {
                break;
            }
        }

        if (propertyElement is null)
        {
            XmlElement propertyGroupElement;
            if (propertyGroups.Count == 0)
            {
                propertyGroupElement = document.CreateElement(PropertyGroupTag);
                document.AppendChild(propertyGroupElement);
            }
            else
            {
                propertyGroupElement = (propertyGroups[0] as XmlElement)!;
            }

            propertyElement = document.CreateElement(propertyName);
            propertyGroupElement.AppendChild(propertyElement);
        }

        if (propertyElement.InnerText != propertyValue)
        {
            propertyChanged = true;
            propertyElement.InnerText = propertyValue;
        }

        return propertyChanged;
    }
}
#endif
