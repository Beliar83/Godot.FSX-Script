namespace GodotSharpGDExtension;

public unsafe static class StringMarshall
{

    public static IntPtr ToNative(string managed)
    {
        IntPtr x = GDExtensionInterface.MemAlloc(8);
        fixed (char* ptr = managed)
        {
            GDExtensionInterface.StringNewWithWideChars(x, managed);
        }
        return x;
    }

    public static string ToManaged(IntPtr str)
    {
        
        var l = (int)GDExtensionInterface.StringToWideChars(str, null, 0);
        return SharpGen.Runtime.StringHelpers.PtrToStringAnsi(str, l);
    }
}
