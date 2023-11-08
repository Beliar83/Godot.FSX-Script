using System.Diagnostics;
using System.Reflection;
using GodotSharpGDExtension;
using SharpGen.Runtime;

namespace GodotSharpGDExtension;

public static unsafe class GDExtensionMain
{
	public static IntPtr Library { get; private set; } = IntPtr.Zero;
	
	// public static GDExtensionInterface extensionInterface;
    private static bool initialized;

    
	public static void Initialize(IntPtr library)
	{
		// Assembly.GetAssembly(typeof(GDExtensionInterface));
		if (initialized) return;
		
		Library = library;
		initialized = true;

		// GDExtensionInterface.

		// GDExtensionInterface.InitInterfaceFunctions(ref extensionInterface);

		// test1 = t1;
		// test2 = t2;
	}
	
	public static IntPtr MoveToUnmanaged<T>(T value) where T : unmanaged
	{
		// PointerSize
		var ptr = GDExtensionInterface.MemAlloc(new PointerSize(sizeof(T)));
		ptr = (IntPtr)(&value);
		return ptr;
	}
	
	public static IntPtr NativeImportResolver(string name, Assembly assembly, DllImportSearchPath? path)
	{
		Console.WriteLine("Attach Debugger");
		while (!Debugger.IsAttached)
		{
		}
		string libraryName;
		if (OperatingSystem.IsWindows())
		{
			libraryName = "godot_sharp_gdextension.dll";
		}
		else if (OperatingSystem.IsLinux())
		{
			libraryName = "godot_sharp_gdextension.so";
		}
		else
		{
			throw new PlatformNotSupportedException();
		}
         
		return name == "godot_sharp_gdextension" ? NativeLibrary.Load($"{libraryName}") : IntPtr.Zero;
	}   	
}