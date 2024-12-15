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
                scriptSession.ParseScript(script.GetSourceCode(), ProjectSettings.GlobalizePath(ScriptPath));
                isUpdated = true;
            }
            else
            {
                if (ResourceLoader.Exists(ScriptPath, "FsxScript"))
                {
                    script ??= ResourceLoader.Load<Script>(ScriptPath, "FsxScript");
                }

                CallDeferred(MethodName.UpdateScript);
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
}
