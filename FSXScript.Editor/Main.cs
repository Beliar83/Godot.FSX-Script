using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FSXScriptCompiler;
using Godot;
using Godot.Bridge;
using ClassDB = Godot.Bridge.ClassDB;

[assembly: DisableRuntimeMarshalling]

namespace FSXScript.Editor;

public class Main
{
    private static ScriptSession? _generalScriptSession;

    [UnmanagedCallersOnly(EntryPoint = "EditorInit")]
    public static bool Init(nint getProcAddress, nint library, nint initialization)
    {
        GodotBridge.Initialize(getProcAddress, library, initialization, config =>
        {
            config.SetMinimumLibraryInitializationLevel(InitializationLevel.Servers);
            config.RegisterInitializer(InitializeEditorTypes);
            config.RegisterTerminator(DeinitializeEditorTypes);
        });

        return true;
    }

    public static void InitializeEditorTypes(InitializationLevel level)
    {
        if (level != InitializationLevel.Scene)
        {
            return;
        }

        ClassDB.RegisterClass<ScriptSession>(ScriptSession.BindMethods);
        _generalScriptSession = new ScriptSession();
        Engine.Singleton.RegisterSingleton(new StringName("GeneralFsxScriptSession"), _generalScriptSession);
    }

    public static void DeinitializeEditorTypes(InitializationLevel level)
    {
        if (level != InitializationLevel.Scene)
        {
            return;
        }

        _generalScriptSession = null;
    }
}
