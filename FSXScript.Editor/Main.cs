using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FSXScriptCompiler;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.FSharp;
using Godot.NativeInterop;
using Godot.NativeInterop.Marshallers;
using Variant = Godot.Variant;

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
        GetValue = &GetValue,
        SetValue = &SetValue,
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
    internal static NativeGodotString GetClassName(IntPtr sessionPointer)
    {
        if (GCHandle.FromIntPtr(sessionPointer).Target is ScriptSession session)
        {
            return NativeGodotString.Create(session.ClassName);
        }

        GD.PrintErr($"Session pointer {sessionPointer} does not point to a valid ScriptSession");
        return NativeGodotString.Create("");
    }

    [UnmanagedCallersOnly]
    internal static void ParseScript(IntPtr sessionPointer, NativeGodotString code)
    {
        GCHandle fromIntPtr = GCHandle.FromIntPtr(sessionPointer);

        if (fromIntPtr.Target is ScriptSession session)
        {
            session.ParseScript(code.ToString());
        }
    }

    [UnmanagedCallersOnly]
    internal static NativeGodotString GetBaseType(IntPtr sessionPointer)
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
                PropertyInfo info = new(field.Name, field.OfType)
                {
                    ClassName = field.OfType == VariantType.Object ? field.OfTypeName : null,
                    Hint = field.PropertyHint,
                    HintString = field.HintText,
                    Usage = field.UsageFlags,
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

    [UnmanagedCallersOnly]
    internal static bool GetValue(IntPtr sessionPointer, NativeGodotStringName* propertyName, NativeGodotVariant* value)
    {
        if (GCHandle.FromIntPtr(sessionPointer).Target is ScriptSession session)
        {
            StringName? name = StringNameMarshaller.ConvertFromUnmanaged(propertyName, false);
            if (!session.PropertyTypes.ContainsKey(name))
            {
                return false;
            }

            if (session.Get(name, out object managedValue))
            {
                switch (session.PropertyTypes[name])
                {
                    case VariantType.Nil:
                        *value = Marshalling.ConvertToVariant(new Variant());
                        break;
                    case VariantType.Bool:
                        *value = Marshalling.ConvertToVariant((bool)managedValue);
                        break;
                    case VariantType.Int:
                        *value = Marshalling.ConvertToVariant((int)managedValue);
                        break;
                    case VariantType.Float:
                        *value = Marshalling.ConvertToVariant((float)managedValue);
                        break;
                    case VariantType.String:
                        *value = Marshalling.ConvertToVariant((string)managedValue);
                        break;
                    case VariantType.Vector2:
                        *value = Marshalling.ConvertToVariant((Vector2)managedValue);
                        break;
                    case VariantType.Vector2I:
                        *value = Marshalling.ConvertToVariant((Vector2I)managedValue);
                        break;
                    case VariantType.Rect2:
                        *value = Marshalling.ConvertToVariant((Rect2)managedValue);
                        break;
                    case VariantType.Rect2I:
                        *value = Marshalling.ConvertToVariant((Rect2I)managedValue);
                        break;
                    case VariantType.Vector3:
                        *value = Marshalling.ConvertToVariant((Vector3)managedValue);
                        break;
                    case VariantType.Vector3I:
                        *value = Marshalling.ConvertToVariant((Vector3I)managedValue);
                        break;
                    case VariantType.Transform2D:
                        *value = Marshalling.ConvertToVariant((Transform2D)managedValue);
                        break;
                    case VariantType.Vector4:
                        *value = Marshalling.ConvertToVariant((Vector4)managedValue);
                        break;
                    case VariantType.Vector4I:
                        *value = Marshalling.ConvertToVariant((Vector4I)managedValue);
                        break;
                    case VariantType.Plane:
                        *value = Marshalling.ConvertToVariant((Plane)managedValue);
                        break;
                    case VariantType.Quaternion:
                        *value = Marshalling.ConvertToVariant((Quaternion)managedValue);
                        break;
                    case VariantType.Aabb:
                        *value = Marshalling.ConvertToVariant((Aabb)managedValue);
                        break;
                    case VariantType.Basis:
                        *value = Marshalling.ConvertToVariant((Basis)managedValue);
                        break;
                    case VariantType.Transform3D:
                        *value = Marshalling.ConvertToVariant((Transform3D)managedValue);
                        break;
                    case VariantType.Projection:
                        *value = Marshalling.ConvertToVariant((Projection)managedValue);
                        break;
                    case VariantType.Color:
                        *value = Marshalling.ConvertToVariant((Color)managedValue);
                        break;
                    case VariantType.StringName:
                        *value = Marshalling.ConvertToVariant((StringName)managedValue);
                        break;
                    case VariantType.NodePath:
                        *value = Marshalling.ConvertToVariant((NodePath)managedValue);
                        break;
                    case VariantType.Rid:
                        *value = Marshalling.ConvertToVariant((Rid)managedValue);
                        break;
                    case VariantType.Object:
                        *value = Marshalling.ConvertToVariant((GodotObject)managedValue);
                        break;
                    case VariantType.Callable:
                        *value = Marshalling.ConvertToVariant((Callable)managedValue);
                        break;
                    case VariantType.Signal:
                        *value = Marshalling.ConvertToVariant((Signal)managedValue);
                        break;
                    case VariantType.Dictionary:
                        *value = Marshalling.ConvertToVariant((GodotDictionary)managedValue);
                        break;
                    case VariantType.Array:
                        *value = Marshalling.ConvertToVariant((GodotArray)managedValue);
                        break;
                    case VariantType.PackedByteArray:
                        *value = Marshalling.ConvertToVariant((PackedByteArray)managedValue);
                        break;
                    case VariantType.PackedInt32Array:
                        *value = Marshalling.ConvertToVariant((PackedInt32Array)managedValue);
                        break;
                    case VariantType.PackedInt64Array:
                        *value = Marshalling.ConvertToVariant((PackedInt64Array)managedValue);
                        break;
                    case VariantType.PackedFloat32Array:
                        *value = Marshalling.ConvertToVariant((PackedFloat32Array)managedValue);
                        break;
                    case VariantType.PackedFloat64Array:
                        *value = Marshalling.ConvertToVariant((PackedFloat64Array)managedValue);
                        break;
                    case VariantType.PackedStringArray:
                        *value = Marshalling.ConvertToVariant((PackedStringArray)managedValue);
                        break;
                    case VariantType.PackedVector2Array:
                        *value = Marshalling.ConvertToVariant((PackedVector2Array)managedValue);
                        break;
                    case VariantType.PackedVector3Array:
                        *value = Marshalling.ConvertToVariant((PackedVector3Array)managedValue);
                        break;
                    case VariantType.PackedColorArray:
                        *value = Marshalling.ConvertToVariant((PackedColorArray)managedValue);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return true;
            }

            return false;
        }

        GD.PrintErr($"Session pointer {sessionPointer} does not point to a valid ScriptSession");
        return false;
    }

    [UnmanagedCallersOnly]
    internal static bool SetValue(IntPtr sessionPointer, NativeGodotStringName* propertyName, NativeGodotVariant* value)
    {
        if (GCHandle.FromIntPtr(sessionPointer).Target is ScriptSession session)
        {
            StringName? name = StringNameMarshaller.ConvertFromUnmanaged(propertyName, false);
            if (!session.PropertyTypes.ContainsKey(name))
            {
                return false;
            }

            switch (session.PropertyTypes[name])
            {
                case VariantType.Nil:
                    session.Set(name, null);
                    break;
                case VariantType.Bool:
                    session.Set(name, Marshalling.ConvertFromVariant<bool>(*value));
                    break;
                case VariantType.Int:
                    session.Set(name, Marshalling.ConvertFromVariant<int>(*value));
                    break;
                case VariantType.Float:
                    session.Set(name, Marshalling.ConvertFromVariant<float>(*value));
                    break;
                case VariantType.String:
                    session.Set(name, Marshalling.ConvertFromVariant<string>(*value));
                    break;
                case VariantType.Vector2:
                    session.Set(name, Marshalling.ConvertFromVariant<Vector2>(*value));
                    break;
                case VariantType.Vector2I:
                    session.Set(name, Marshalling.ConvertFromVariant<Vector2I>(*value));
                    break;
                case VariantType.Rect2:
                    session.Set(name, Marshalling.ConvertFromVariant<Rect2>(*value));
                    break;
                case VariantType.Rect2I:
                    session.Set(name, Marshalling.ConvertFromVariant<Rect2I>(*value));
                    break;
                case VariantType.Vector3:
                    session.Set(name, Marshalling.ConvertFromVariant<Vector3>(*value));
                    break;
                case VariantType.Vector3I:
                    session.Set(name, Marshalling.ConvertFromVariant<Vector3I>(*value));
                    break;
                case VariantType.Transform2D:
                    session.Set(name, Marshalling.ConvertFromVariant<Transform2D>(*value));
                    break;
                case VariantType.Vector4:
                    session.Set(name, Marshalling.ConvertFromVariant<Vector4>(*value));
                    break;
                case VariantType.Vector4I:
                    session.Set(name, Marshalling.ConvertFromVariant<Vector4I>(*value));
                    break;
                case VariantType.Plane:
                    session.Set(name, Marshalling.ConvertFromVariant<Plane>(*value));
                    break;
                case VariantType.Quaternion:
                    session.Set(name, Marshalling.ConvertFromVariant<Quaternion>(*value));
                    break;
                case VariantType.Aabb:
                    session.Set(name, Marshalling.ConvertFromVariant<Aabb>(*value));
                    break;
                case VariantType.Basis:
                    session.Set(name, Marshalling.ConvertFromVariant<Basis>(*value));
                    break;
                case VariantType.Transform3D:
                    session.Set(name, Marshalling.ConvertFromVariant<Transform3D>(*value));
                    break;
                case VariantType.Projection:
                    session.Set(name, Marshalling.ConvertFromVariant<Projection>(*value));
                    break;
                case VariantType.Color:
                    session.Set(name, Marshalling.ConvertFromVariant<Color>(*value));
                    break;
                case VariantType.StringName:
                    session.Set(name, Marshalling.ConvertFromVariant<StringName>(*value));
                    break;
                case VariantType.NodePath:
                    session.Set(name, Marshalling.ConvertFromVariant<NodePath>(*value));
                    break;
                case VariantType.Rid:
                    session.Set(name, Marshalling.ConvertFromVariant<Rid>(*value));
                    break;
                case VariantType.Object:
                    session.Set(name, Marshalling.ConvertFromVariant<GodotObject>(*value));
                    break;
                case VariantType.Callable:
                    session.Set(name, Marshalling.ConvertFromVariant<Callable>(*value));
                    break;
                case VariantType.Signal:
                    session.Set(name, Marshalling.ConvertFromVariant<Signal>(*value));
                    break;
                case VariantType.Dictionary:
                    session.Set(name, Marshalling.ConvertFromVariant<GodotDictionary>(*value));
                    break;
                case VariantType.Array:
                    session.Set(name, Marshalling.ConvertFromVariant<GodotArray>(*value));
                    break;
                case VariantType.PackedByteArray:
                    session.Set(name, Marshalling.ConvertFromVariant<PackedByteArray>(*value));
                    break;
                case VariantType.PackedInt32Array:
                    session.Set(name, Marshalling.ConvertFromVariant<PackedInt32Array>(*value));
                    break;
                case VariantType.PackedInt64Array:
                    session.Set(name, Marshalling.ConvertFromVariant<PackedInt64Array>(*value));
                    break;
                case VariantType.PackedFloat32Array:
                    session.Set(name, Marshalling.ConvertFromVariant<PackedFloat32Array>(*value));
                    break;
                case VariantType.PackedFloat64Array:
                    session.Set(name, Marshalling.ConvertFromVariant<PackedFloat64Array>(*value));
                    break;
                case VariantType.PackedStringArray:
                    session.Set(name, Marshalling.ConvertFromVariant<PackedStringArray>(*value));
                    break;
                case VariantType.PackedVector2Array:
                    session.Set(name, Marshalling.ConvertFromVariant<PackedVector2Array>(*value));
                    break;
                case VariantType.PackedVector3Array:
                    session.Set(name, Marshalling.ConvertFromVariant<PackedVector3Array>(*value));
                    break;
                case VariantType.PackedVector4Array:
                    session.Set(name, Marshalling.ConvertFromVariant<PackedVector4Array>(*value));
                    break;
                case VariantType.PackedColorArray:
                    session.Set(name, Marshalling.ConvertFromVariant<PackedColorArray>(*value));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return true;
        }

        GD.PrintErr($"Session pointer {sessionPointer} does not point to a valid ScriptSession");
        return false;
    }
}
