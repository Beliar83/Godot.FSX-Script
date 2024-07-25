using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FSXScriptCompiler;
using Godot;
using Godot.Bridge;
using Godot.FSharp;
using Godot.NativeInterop;

[assembly: DisableRuntimeMarshalling]

namespace FSXScript.Editor;

public unsafe class Main
{
    // TODO: Free handles
    private static readonly ConcurrentBag<GCHandle> Handles = [];

    private static readonly DotnetMethods DotnetMethods = new()
    {
        InitGodot = &GodotBridge.Initialize,
        InitFsxScript = &InitFsxScript,
        CreateSession = &CreateSession,
        GetClassName = &GetClassName,
        ParseScript = &ParseScript,
        GetBaseType = &GetBaseType,
        GetPropertyList = &GetPropertyList,
    };

    [UnmanagedCallersOnly]
    public static DotnetMethods GetMethods()
    {
        return DotnetMethods;
    }

    [UnmanagedCallersOnly]
    internal static void InitFsxScript(NativeGodotString basePath)
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

    [UnmanagedCallersOnly]
    internal static GDExtensionPropertyInfo* GetPropertyList(IntPtr sessionPointer, uint* count)
    {
        PropertyInfoList propertyInfoList = [];

        if (GCHandle.FromIntPtr(sessionPointer).Target is ScriptSession session)
        {
            foreach (ObjectGenerator.Field field in session.PropertyList)
            {
                PropertyInfo info = new(new StringName(field.Name), (VariantType)field.OfType)
                {
                    ClassName = field.OfType == GodotStubs.Type.Object ? new StringName(field.OfTypeName) : null,
                    Hint = (PropertyHint)field.PropertyHint,
                    HintString = field.HintText,
                    Usage = (PropertyUsageFlags)field.UsageFlags,
                };

                propertyInfoList.Add(info);
            }
        }
        else
        {
            GD.PrintErr($"Session pointer {sessionPointer} does not point to a valid ScriptSession");
        }

        *count = (uint)propertyInfoList.Count;
        return PropertyInfoList.ConvertToNative(propertyInfoList);
    }
}
