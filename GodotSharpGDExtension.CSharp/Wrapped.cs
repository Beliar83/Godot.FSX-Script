namespace GodotSharpGDExtension;

public unsafe class Wrapped
{

    public IntPtr internalPointer;

    protected Wrapped(StringName type)
    {
        Console.WriteLine(type.Capitalize());
        internalPointer = GDExtensionInterface.ClassdbConstructObject(type.internalPointer);
    }
    protected Wrapped(IntPtr data) => internalPointer = data;
    public static unsafe void __Notification(void* instance, int what) { }
    public static unsafe void* __RegisterVirtual(void* userData, void* stringData) { return null; }

    public static void Register() { }
}
