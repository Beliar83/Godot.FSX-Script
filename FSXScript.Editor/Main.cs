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
        stringTest = &Method4.GetNameIntoRustVec,
        fromRust = &Method4.StringFromRust,
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

    // GD.Print(_handles.c(sessionPointer));
    // var bytes = new byte[16];
    // Marshal.Copy((IntPtr)sessionPointer, bytes, 0, 16);
    // var guid = new Guid(bytes);
    //
    // string aggregate = String.Join(", ", bytes.Select(b => b.ToString()));
    // GD.Print($"{guid} - {_scriptSessions.ContainsKey(guid)}");
    // GD.Print($"{aggregate}");
    //
    // return new PackedStringArray().NativeValue;

    // if (_scriptSessions.TryGetValue(sessionPointer, out ScriptSession? session))
    // {
    //     FSharpList<string> messages = session.ParseScript(code.ToString());
    //     GD.Print("Got messages");
    //     return new PackedStringArray(messages).NativeValue;
    // }
    // else
    // {
    //     return new PackedStringArray([$"Session pointer {sessionPointer} does not point to a valid ScriptSession"]).NativeValue;
    // }
}

public static class Method4
{
    [UnmanagedCallersOnly]
    public static NativeGodotString GetNameIntoRustVec()
    {
        // ScriptSession scriptSession = new ScriptSession();
        // var test = NativeGodotString.Create(scriptSession.BuildDummy("Test"));
        // return test;
        var name = "Some string we want to return to Rust.";
        return NativeGodotString.Create(name);
    }

    [UnmanagedCallersOnly]
    public static void StringFromRust(NativeGodotString godotString)
    {
        // var variant = new NativeGodotVariant { String = godotString, Type = VariantType.String };
        GD.Print(godotString.ToString());
    }
}
