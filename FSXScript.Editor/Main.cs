using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FSXScriptCompiler;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

[assembly: DisableRuntimeMarshalling]

namespace FSXScript.Editor;

public unsafe class Main
{
    // TODO: Free handles
    private static readonly ConcurrentBag<GCHandle> Handles = [];

    private static readonly DotnetMethods DotnetMethods = new()
    {
        Init = &GodotBridge.Initialize,
        SetBasePath = &SetBasePath,
        CreateSession = &CreateSession,
        GetClassName = &GetClassName,
        ParseScript = &ParseScript,
        GetBaseType = &GetBaseType,
    };

    [UnmanagedCallersOnly]
    public static DotnetMethods GetMethods()
    {
        return DotnetMethods;
    }

    [UnmanagedCallersOnly]
    public static void SetBasePath(NativeGodotString basePath)
    {
        ScriptSession.BasePath = basePath.ToString();
    }

    [UnmanagedCallersOnly]
    public static IntPtr CreateSession()
    {
        ScriptSession scriptSession = new();
        GCHandle gcHandle = GCHandle.Alloc(scriptSession);
        Handles.Add(gcHandle);
        return GCHandle.ToIntPtr(gcHandle);
    }

    [UnmanagedCallersOnly]
    public static NativeGodotString GetClassName(IntPtr sessionPointer)
    {
        if (GCHandle.FromIntPtr(sessionPointer).Target is ScriptSession session)
        {
            return NativeGodotString.Create(session.ClassName);
        }

        GD.PrintErr($"Session pointer {sessionPointer} does not point to a valid ScriptSession");
        return NativeGodotString.Create("");
    }

    [UnmanagedCallersOnly]
    public static void ParseScript(IntPtr sessionPointer, NativeGodotString code)
    {
        GCHandle fromIntPtr = GCHandle.FromIntPtr(sessionPointer);

        if (fromIntPtr.Target is ScriptSession session)
        {
            session.ParseScript(code.ToString());
        }
    }

    [UnmanagedCallersOnly]
    public static NativeGodotString GetBaseType(IntPtr sessionPointer)
    {
        if (GCHandle.FromIntPtr(sessionPointer).Target is ScriptSession session)
        {
            return NativeGodotString.Create(session.BaseType);
        }

        GD.PrintErr($"Session pointer {sessionPointer} does not point to a valid ScriptSession");
        return NativeGodotString.Create("");
    }    

}