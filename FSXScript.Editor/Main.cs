using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FSXScriptInterpreter;
using Godot;
using Godot.Bridge;
using ClassDB = Godot.Bridge.ClassDB;

[assembly: DisableGodotEntryPointGeneration]
[assembly: DisableRuntimeMarshalling]

namespace FSXScript.Editor;

public static class Main
{
    private static FsxScriptLanguage? fsxScriptLanguage;
    private static FsxScriptResourceFormatSaver? fsxScriptResourceFormatSaver;
    private static FsxScriptResourceFormatLoader? fsxScriptResourceFormatLoader;

    private static unsafe void InitializeFsxScriptTypes(InitializationLevel level)
    {
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (level)
        {
            case InitializationLevel.Scene:
                ClassDB.RegisterClass<FsxScript>(FsxScript.BindMethods);
                ClassDB.RegisterClass<FsxScriptLanguage>(FsxScriptLanguage.BindMethods);
                ClassDB.RegisterClass<FsxScriptResourceFormatSaver>(FsxScriptResourceFormatSaver.BindMethods);
                ClassDB.RegisterClass<FsxScriptResourceFormatLoader>(FsxScriptResourceFormatLoader.BindMethods);
                fsxScriptLanguage = new FsxScriptLanguage();
                Engine.Singleton.RegisterScriptLanguage(fsxScriptLanguage);
                Engine.Singleton.RegisterSingleton(FsxScript.LanguageName, fsxScriptLanguage);
                fsxScriptResourceFormatSaver = new FsxScriptResourceFormatSaver();
                ResourceSaver.Singleton.AddResourceFormatSaver(fsxScriptResourceFormatSaver);
                fsxScriptResourceFormatLoader = new FsxScriptResourceFormatLoader();
                ResourceLoader.Singleton.AddResourceFormatLoader(fsxScriptResourceFormatLoader);
                break;
            case InitializationLevel.Editor:
                 // if (!LspService.LspService.StartLsp())
                // {
                //     GD.PrintErr("Could not start LSP service");
                // }

                break;
        }
    }

    public static void DeinitializeFsxScriptTypes(InitializationLevel level)
    {
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (level)
        {
            case InitializationLevel.Scene:
                ResourceLoader.Singleton.RemoveResourceFormatLoader(fsxScriptResourceFormatLoader);
                ResourceSaver.Singleton.RemoveResourceFormatSaver(fsxScriptResourceFormatSaver);
                Engine.Singleton.UnregisterSingleton(FsxScript.LanguageName);
                Engine.Singleton.UnregisterScriptLanguage(fsxScriptLanguage);
                fsxScriptLanguage = null;
                fsxScriptResourceFormatSaver = null;
                break;
            case InitializationLevel.Editor:
                // LspService.LspService.ShutdownAndExit();
                break;
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "fsx_script_init")]
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
