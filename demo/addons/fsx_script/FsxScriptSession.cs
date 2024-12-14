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

    public Array<Dictionary> GetProperties()
    {
        while (!isUpdated)
        {
            UpdateScript();
            Task.Delay(1).Wait();
        }

        Array<Dictionary> properties = scriptSession.GetProperties();
        return properties;
    }

    public bool HasProperty(StringName name)
    {
        while (!isUpdated)
        {
            UpdateScript();
            Task.Delay(1).Wait();
        }

        return scriptSession.HasProperty(name);
    }

    public StringName GetClassName()
    {
        return scriptSession.GetClassName();
    }

    public StringName GetBaseType()
    {
        return scriptSession.GetBaseType();
    }

    public Dictionary Validate(string script, string path, bool validateFunctions, bool validateErrors,
        bool validateWarnings, bool validateSafeLines)
    {
        return scriptSession.Validate(script, path, validateFunctions, validateErrors, validateWarnings,
            validateSafeLines);
    }
}
