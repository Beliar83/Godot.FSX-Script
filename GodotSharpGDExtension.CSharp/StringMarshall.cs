namespace GodotSharpGDExtension;

public static class StringMarshall
{

    public static IntPtr ToNative(string managed)
    {
        return GDExtensionInterface.StringNewWithWideChars(managed);
    }

    public static string ToManaged(IntPtr str)
    {
        
        var l = (int)GDExtensionInterface.StringToWideChars(str, IntPtr.Zero, 0);
        return SharpGen.Runtime.StringHelpers.PtrToStringAnsi(str, l);
    }
}
