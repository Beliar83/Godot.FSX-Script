using System.Runtime.InteropServices;
using Godot.NativeInterop;

namespace FSXScript.Editor;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct DotnetMethods
{
    internal delegate* unmanaged<GDExtensionInterface*, void> Init;
    internal delegate* unmanaged<NativeGodotString, void> SetBasePath;
    internal delegate* unmanaged<IntPtr> CreateSession;
    internal delegate* unmanaged<IntPtr, NativeGodotString> GetClassName;
    internal delegate* unmanaged<IntPtr, NativeGodotString, void> ParseScript;
    internal delegate* unmanaged<IntPtr, NativeGodotString> GetBaseType;
}
