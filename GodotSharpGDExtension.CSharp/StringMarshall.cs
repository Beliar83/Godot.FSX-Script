namespace GodotSharpGDExtension;

public unsafe static class StringMarshall
{

    public static void* ToNative(string managed)
    {
        var x = GDExtensionMain.extensionInterface.mem_alloc.Data.Pointer(8);
        fixed (char* ptr = managed)
        {
            GDExtensionMain.extensionInterface.string_new_with_utf16_chars.Data.Pointer(x, (GDExtensionInterface.char16_t*)ptr);
        }
        return x;
    }

    public static string ToManaged(void* str)
    {
        var l = (int)GDExtensionMain.extensionInterface.string_to_utf16_chars.Data.Pointer(str, null, 0);
        var span = stackalloc char[l];
        GDExtensionMain.extensionInterface.string_to_utf16_chars.Data.Pointer(str, (GDExtensionInterface.char16_t*)span, l);
        return new string(span, 0, l);
    }
}
