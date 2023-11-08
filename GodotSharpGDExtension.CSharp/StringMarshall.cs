namespace GodotSharpGDExtension;

public static class StringMarshall
{

    public static IntPtr ToNative(string managed)
    {
        return GDExtensionInterface.StringNewWithUtf8Chars(managed);
    }

    public static string ToManaged(IntPtr str)
    {
        // GDExtensionInterface.str
        var l = (int)GDExtensionInterface.StringToUtf8Chars(str, IntPtr.Zero, 0);
        return SharpGen.Runtime.StringHelpers.PtrToStringAnsi(str, l);
    }
}
