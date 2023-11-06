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
		// Console.WriteLine("Attach Debugger");
		// while (!Debugger.IsAttached)
		// {
		// }
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
	
	
}