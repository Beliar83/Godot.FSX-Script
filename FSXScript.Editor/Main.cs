using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FSXScriptInterpreter;
using Godot;
using Godot.Bridge;
using ClassDB = Godot.Bridge.ClassDB;

[assembly: DisableGodotEntryPointGeneration]
[assembly: DisableRuntimeMarshalling]

namespace FSXScript.Editor;

public class Main
{
    private static FsxScriptLanguage? _fsxScriptLanguage;
    private static FsxScriptResourceFormatSaver? _fsxScriptResourceFormatSaver;
    
    public static void InitializeFsxScriptTypes(InitializationLevel level)
    {
        if (level != InitializationLevel.Scene)
        {
            return;
        }
        
        ClassDB.RegisterClass<FsxScript>(FsxScript.BindMethods);
        ClassDB.RegisterClass<FsxScriptLanguage>(FsxScriptLanguage.BindMethods);
        ClassDB.RegisterClass<FsxScriptResourceFormatSaver>(FsxScriptResourceFormatSaver.BindMethods);
        _fsxScriptLanguage = new FsxScriptLanguage();
        Engine.Singleton.RegisterScriptLanguage(_fsxScriptLanguage);
        Engine.Singleton.RegisterSingleton(FsxScript.LanguageName, _fsxScriptLanguage);
        _fsxScriptResourceFormatSaver = new FsxScriptResourceFormatSaver();
        ResourceSaver.Singleton.AddResourceFormatSaver(_fsxScriptResourceFormatSaver);

    }

    public static void DeinitializeFsxScriptTypes(InitializationLevel level)
    {
        if (level != InitializationLevel.Scene)
        {
            return;
        }
        ResourceSaver.Singleton.RemoveResourceFormatSaver(_fsxScriptResourceFormatSaver);
        Engine.Singleton.UnregisterSingleton(FsxScript.LanguageName);
        Engine.Singleton.UnregisterScriptLanguage(_fsxScriptLanguage);
        _fsxScriptLanguage = null;
        _fsxScriptResourceFormatSaver = null;
    }

    [UnmanagedCallersOnly]
    public static bool Init(nint getProcAddress, nint library, nint initialization)
    {
        GodotBridge.Initialize(getProcAddress, library, initialization, config =>
        {
            config.SetMinimumLibraryInitializationLevel(InitializationLevel.Scene);
            config.RegisterInitializer(InitializeFsxScriptTypes);
            config.RegisterTerminator(DeinitializeFsxScriptTypes);
        });

        return true;
    }
}