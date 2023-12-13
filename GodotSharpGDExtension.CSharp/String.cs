using System.Runtime.CompilerServices;
using System.Text;

namespace GodotSharpGDExtension;

public unsafe partial class String : IDisposable
{
    private string? value;

    public static implicit operator String(string value)
    {
        // // ushort[] charArray = value.Append('\0').Select(c => (ushort)c).ToArray();
        // // GCHandle handle = GCHandle.Alloc(charArray, GCHandleType.Pinned);
        // byte[] utf16Bytes = Encoding.Unicode.GetBytes(value);
        //
        // GCHandle handle = GCHandle.Alloc(utf16Bytes, GCHandleType.Pinned);
        //
        // var godotWideString = new GodotUTF16String
        // {
        //     data = (ushort*)handle.AddrOfPinnedObject(),
        //     length = (nuint)utf16Bytes.Length,
        //     ownership = Ownership.Managed,
        // };        
        //
        // // var godotWideString = new GodotWideString
        // // {
        // //     data = (ushort*)handle.AddrOfPinnedObject(),
        // //     length = (nuint)charArray.Length,
        // //     ownership = Ownership.Managed,
        // // };
        //
        // GodotString converted = NativeMethods.convert_string_from_utf16_string(godotWideString, value.Length);
        // return new String(converted.internal_pointer )
        // {
        //     length = utf16Bytes.Length,
        //     dataUTF16 = godotWideString,
        //     handle = handle,
        // };
        
        // Get the number of characters in the string
        int charCount = value.Length;

        byte[] bytes = charCount == 0 ? Encoding.Unicode.GetBytes("\0\0"): Encoding.Unicode.GetBytes(value);
        fixed (byte* data = bytes)
        {
            // Pass the pointer to the UTF-16 string and the character count to Godot
            GodotString stringFromUtf16String = NativeMethods.convert_string_from_utf16_string((ushort*)data, charCount);
            return new String((__GdextType*)stringFromUtf16String.internal_pointer) { value = value};
        }
    }

    public static implicit operator string(String value)
    {
        
        // Godot strings are immutable, so if we have data it should not have changed.
        if (value.value is not null)
        {
            return value.value;
        }

        GodotUTF16String utf16String = NativeMethods.convert_godot_string_to_utf16_string(new GodotString { internal_pointer = (__GdextString*)value.InternalPointer });

        if (utf16String.length == 0) return "";
        
        value.value = Encoding.Unicode.GetString((byte*)utf16String.data, (int)utf16String.length);
        return value.value;
    }
    
    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
