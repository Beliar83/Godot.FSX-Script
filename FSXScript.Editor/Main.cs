using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FSXScriptCompiler;
using Godot.Bridge;
using ClassDB = Godot.Bridge.ClassDB;

[assembly: DisableRuntimeMarshalling]

namespace FSXScript.Editor;

public class Main
{
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
        if (level != InitializationLevel.Servers)
        {
            return;
        }

        ClassDB.RegisterClass<ScriptSession>(ScriptSession.BindMethods);
    }

    public static void DeinitializeEditorTypes(InitializationLevel level)
    { }
}
