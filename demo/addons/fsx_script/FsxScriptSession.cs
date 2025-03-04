#if TOOLS
#if FSX_SERVER_ACTIVE
#nullable enable
using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using Godot.FSharp;

namespace FsxScriptAddon;

[GlobalClass]
public partial class FsxScriptSession : GodotObject
{
    [Export] public string? ScriptPath { get; set; }
    private readonly ScriptSession scriptSession = new();
    private Script? script;
    private bool isUpdated;
    private bool isParsed;
    private bool isUpdating;

    private void ScriptCodeChanged()
    {
        isParsed = false;
        if (script is not null && !string.IsNullOrWhiteSpace(ScriptPath))
        {
            string scriptPath = ProjectSettings.GlobalizePath(ScriptPath);
            string sourceCode = script.GetSourceCode();
            scriptSession.ParseScript(sourceCode, scriptPath);
            isParsed = true;
        }

        isUpdated = false;
    }

    internal void UpdateScript()
    {
        if (isUpdating)
        {
            return;
        }

        isUpdated = false;
        isUpdating = true;
        try
        {
            if (script is not null && !string.IsNullOrWhiteSpace(ScriptPath))
            {
                string scriptPath = ProjectSettings.GlobalizePath(ScriptPath);
                Dictionary<InteropInstance, Dictionary> storedScripts =
                    Interop.Unload(ScriptPath, out WeakReference? contextReference);

                while (contextReference?.IsAlive ?? false)
                {
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                    GC.WaitForPendingFinalizers();
                    Task.Delay(1).Wait();
                }

                string sourceCode = script.GetSourceCode();
                if (!isParsed)
                {
                    scriptSession.ParseScript(sourceCode, scriptPath);
                }

                scriptSession.Compile(sourceCode, scriptPath);
                contextReference = Interop.Load(ScriptPath, scriptSession.GetFullTypeName(), GetBaseType(),
                    storedScripts);
                while (contextReference?.IsAlive ?? false)
                {
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                    GC.WaitForPendingFinalizers();
                    Task.Delay(1).Wait();
                }

                isUpdated = true;
            }
            else
            {
                if (ResourceLoader.Exists(ScriptPath, "FsxScript"))
                {
                    script ??= ResourceLoader.Load<Script>(ScriptPath, "FsxScript");
                }

                CallDeferred(nameof(UpdateScript));
            }
        }
        finally
        {
            isUpdating = false;
        }
    }

    private bool HasFsxMethod(StringName methodName)
    {
        while (!isUpdated)
        {
            UpdateScript();
            Task.Delay(1).Wait();
        }

        return scriptSession.HasMethod(methodName);
    }

    private Array<Dictionary> GetProperties()
    {
        while (!isUpdated)
        {
            UpdateScript();
            Task.Delay(1).Wait();
        }

        Array<Dictionary> properties = scriptSession.GetProperties();
        return properties;
    }

    private Array<Dictionary> GetMethods()
    {
        while (!isUpdated)
        {
            UpdateScript();
            Task.Delay(1).Wait();
        }

        return scriptSession.GetMethods();
    }

    private bool HasProperty(StringName name)
    {
        while (!isUpdated)
        {
            UpdateScript();
            Task.Delay(1).Wait();
        }

        return scriptSession.HasProperty(name);
    }

    internal StringName GetClassName()
    {
        return scriptSession.GetClassName();
    }

    private StringName GetBaseType()
    {
        return scriptSession.GetBaseType();
    }

    private Dictionary Validate(string scriptCode, string path, bool validateFunctions, bool validateErrors,
        bool validateWarnings, bool validateSafeLines)
    {
        return ScriptSession.Validate(scriptCode, path, validateFunctions, validateErrors, validateWarnings,
            validateSafeLines);
    }

    private Dictionary Complete(string scriptCode)
    {
        return scriptSession.Complete(scriptCode);
    }

    private Dictionary Lookup(int line, int column, string lineText, string symbol)
    {
        return scriptSession.Lookup(line, column, lineText, symbol);
    }

    private bool CanInstantiate()
    {
        while (!isUpdated)
        {
            UpdateScript();
            Task.Delay(1).Wait();
        }

        return scriptSession.CanInstantiate();
    }

    private bool IsTool()
    {
        return scriptSession.IsTool;
    }
}
#endif
#endif
