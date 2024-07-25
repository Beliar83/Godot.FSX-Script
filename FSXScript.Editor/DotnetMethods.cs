using System.Runtime.InteropServices;
using Godot.NativeInterop;

namespace FSXScript.Editor;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct DotnetMethods
{
    internal delegate* unmanaged<GDExtensionInterface*, void> InitGodot;
    internal delegate* unmanaged<NativeGodotString, void> InitFsxScript;
    internal delegate* unmanaged<IntPtr> CreateSession;
    internal delegate* unmanaged<IntPtr, NativeGodotString> GetClassName;
    internal delegate* unmanaged<IntPtr, NativeGodotString, void> ParseScript;
    internal delegate* unmanaged<IntPtr, NativeGodotString> GetBaseType;
    internal delegate* unmanaged<IntPtr, uint*, Godot.NativeInterop.GDExtensionPropertyInfo*> GetPropertyList;
}
