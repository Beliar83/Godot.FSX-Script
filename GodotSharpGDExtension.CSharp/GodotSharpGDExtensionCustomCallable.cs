using System.Runtime.CompilerServices;

namespace GodotSharpGDExtension;

internal static unsafe class GodotSharpGDExtensionCustomCallable
{
    [DllImport("godot_android", EntryPoint = "libgodot_create_callable", CallingConvention = CallingConvention.StdCall)]
    internal static extern IntPtr Android_libgodot_create_callable(IntPtr customobject);

    [DllImport("libgodot", EntryPoint = "libgodot_create_callable", CallingConvention = CallingConvention.StdCall)]
    internal static extern IntPtr Desktop_libgodot_create_callable(IntPtr customobject);


    [DllImport("godot_android", EntryPoint = "libgodot_bind_custom_callable", CallingConvention = CallingConvention.StdCall)]
    internal static extern void Android_libgodot_bind_custom_callable(void* callableHashBind, void* getAsTextBind, void* getObjectBind, void* disposesBind, void* callBind);

    [DllImport("libgodot", EntryPoint = "libgodot_bind_custom_callable", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void Desktop_libgodot_bind_custom_callable(void* callableHashBind, void* getAsTextBind, void* getObjectBind, void* disposesBind, void* callBind);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Libgodot_bind_custom_callable(void* callableHashBind, void* getAsTextBind, void* getObjectBind, void* disposesBind, void* callBind)
    {
        if (AndroidTest.Check())
        {
            Android_libgodot_bind_custom_callable(callableHashBind, getAsTextBind, getObjectBind, disposesBind, callBind);
        }
        else
        {
            Desktop_libgodot_bind_custom_callable(callableHashBind, getAsTextBind, getObjectBind, disposesBind, callBind);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IntPtr Libgodot_create_callable(IntPtr customobject)
    {
        if (AndroidTest.Check())
        {
            return Android_libgodot_create_callable(customobject);
        }
        else
        {
            return Desktop_libgodot_create_callable(customobject);
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate uint CallableHashBindDel(void* targetObject);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate void* GetAsTextBindDel(void* targetObject);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate ulong GetObjectBindDel(void* targetObject);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate void DisposesBindDel(void* targetObject);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate void CallBindDel(void* targetObject, void** args, int argsLength, void* returnData, void* error);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Callable GetCallable(void* customobject)
    {
        return (Callable)GCHandle.FromIntPtr(new IntPtr(customobject)).Target;
    }

    internal static uint Callable_hash_bind(void* targetObject)
    {
        var target = GetCallable(targetObject);
        return (uint)(target.@delegate?.GetHashCode() ?? target.GetHashCode());
    }

    internal static IntPtr Get_as_text_bind_del(void* targetObject)
    {
        var target = GetCallable(targetObject);
        return StringMarshall.ToNative(target.@delegate?.ToString() ?? "null");
    }

    internal static ulong Get_object_bind_del(void* targetObject)
    {
        var target = GetCallable(targetObject);
        return (ulong)target.godotObject.GetInstanceId();
    }

    internal static void Disposes_bind_del(void* targetObject)
    {
        GetCallable(targetObject).Free();
    }

    internal static void Call_bind_del(void* targetObject, IntPtr* args, int argsLength, IntPtr returnData, void* error)
    {
        var target = GetCallable(targetObject);
        var callargs = new object[argsLength];
        for (int i = 0; i < argsLength; i++)
        {
            var varenet = new Variant(args[i]);
            callargs[i] = Variant.VariantToObject(varenet);
        }
        var output = target.@delegate.DynamicInvoke(callargs);
        Variant.SaveIntoPointer(output, returnData);
    }

    internal static void Init()
    {
        var callableHashPointer = (void*)SaftyRapper.GetFunctionPointerForDelegate(Callable_hash_bind);
        var getAsTextPointer = (void*)SaftyRapper.GetFunctionPointerForDelegate(Get_as_text_bind_del);
        var getObjectPointer = (void*)SaftyRapper.GetFunctionPointerForDelegate(Get_object_bind_del);
        var disposesPointer = (void*)SaftyRapper.GetFunctionPointerForDelegate(Disposes_bind_del);
        var callPointer = (void*)SaftyRapper.GetFunctionPointerForDelegate(Call_bind_del);
        Libgodot_bind_custom_callable(callableHashPointer, getAsTextPointer, getObjectPointer, disposesPointer, callPointer);
    }
}