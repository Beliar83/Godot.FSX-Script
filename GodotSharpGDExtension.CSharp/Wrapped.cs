namespace GodotSharpGDExtension;

public unsafe class Wrapped : TypedGodotType<Wrapped>
{
    // protected Wrapped(StringName type)
    // {
    //     // Console.WriteLine(type.Capitalize());
    //     InternalPointer = GDExtensionMain.extensionInterface.classdb_construct_object.Data.Pointer(type.InternalPointer);
    // }
    protected Wrapped(IntPtr data) => InternalPointer = data;
    public static unsafe void __Notification(void* instance, int what) { }
    public static unsafe void* __RegisterVirtual(void* userData, void* stringData) { return null; }

    public static void Register() { }
}

public unsafe class GodotObject : Wrapped
{
    /// <inheritdoc />
    internal GodotObject(IntPtr data) : base(data)
    { }
}