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
        if (level != InitializationLevel.Scene)
        {
            return;
        }

        HostFxr.LoadAssembly("addons/fsx-script/bin/FSXScriptInterpreter.Interop.runtimeconfig.json");

        HostFxr.LoadAssemblyAndGetFunctionPointerForUnmanagedCallersOnly(
            "addons/fsx-script/bin/FSXScriptInterpreter.Interop.dll",
            "FSXScriptInterpreter.Interop.Interop, FSXScriptInterpreter.Interop", "CreateScriptSession",
            out IntPtr functionPointer);
        delegate* unmanaged<GCHandle> createScriptSession = (delegate* unmanaged<GCHandle>)functionPointer;
        HostFxr.LoadAssemblyAndGetFunctionPointerForUnmanagedCallersOnly(
            "addons/fsx-script/bin/FSXScriptInterpreter.Interop.dll",
            "FSXScriptInterpreter.Interop.Interop, FSXScriptInterpreter.Interop", "ParseScript", out functionPointer);
        delegate* unmanaged<GCHandle, ushort*, void> parseScript =
            (delegate* unmanaged<GCHandle, ushort*, void>)functionPointer;
        HostFxr.LoadAssemblyAndGetFunctionPointerForUnmanagedCallersOnly(
            "addons/fsx-script/bin/FSXScriptInterpreter.Interop.dll",
            "FSXScriptInterpreter.Interop.Interop, FSXScriptInterpreter.Interop", "GetClassName", out functionPointer);
        delegate* unmanaged<GCHandle, ushort*> getClassName = (delegate* unmanaged<GCHandle, ushort*>)functionPointer;
        HostFxr.LoadAssemblyAndGetFunctionPointerForUnmanagedCallersOnly(
            "addons/fsx-script/bin/FSXScriptInterpreter.Interop.dll",
            "FSXScriptInterpreter.Interop.Interop, FSXScriptInterpreter.Interop", "DestroyScriptSession",
            out functionPointer);
        delegate* unmanaged<GCHandle, void> destroyScriptSession = (delegate* unmanaged<GCHandle, void>)functionPointer;

        ScriptSession.InteropFunctions =
            new ScriptSessionInteropFunctions(createScriptSession, parseScript, getClassName, destroyScriptSession);

        FsxScript.CreateSession = ScriptSession.CreateScriptSession;


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
    }

    public static void DeinitializeFsxScriptTypes(InitializationLevel level)
    {
        if (level != InitializationLevel.Scene)
        {
            return;
        }

        ResourceLoader.Singleton.RemoveResourceFormatLoader(fsxScriptResourceFormatLoader);
        ResourceSaver.Singleton.RemoveResourceFormatSaver(fsxScriptResourceFormatSaver);
        Engine.Singleton.UnregisterSingleton(FsxScript.LanguageName);
        Engine.Singleton.UnregisterScriptLanguage(fsxScriptLanguage);
        fsxScriptLanguage = null;
        fsxScriptResourceFormatSaver = null;
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
