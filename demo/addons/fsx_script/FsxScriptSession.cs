#if TOOLS
using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using Godot.FSharp;

namespace FsxScript;

[GlobalClass]
public partial class FsxScriptSession : GodotObject
{
    [Export] public string? ScriptPath { get; set; }
    private readonly ScriptSession scriptSession = new();
    private Script? script;
    private bool isUpdated;
    private bool isUpdating;

    private void UpdateScript()
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
                scriptSession.ParseScript(sourceCode, scriptPath);
                scriptSession.Compile(sourceCode, scriptPath);
                contextReference = Interop.Load(ScriptPath, scriptSession.GetClassName(), GetBaseType(), storedScripts);
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

    private bool HasProperty(StringName name)
    {
        while (!isUpdated)
        {
            UpdateScript();
            Task.Delay(1).Wait();
        }

        return scriptSession.HasProperty(name);
    }

    private StringName GetClassName()
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

    private bool CanInstantiate()
    {
        while (!isUpdated)
        {
            UpdateScript();
            Task.Delay(1).Wait();
        }

        return scriptSession.CanInstantiate();
    }
}
#endif
