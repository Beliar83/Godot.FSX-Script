using System.Diagnostics;
using System.Reflection;
using GodotSharpGDExtension;
using SharpGen.Runtime;

namespace GodotSharpGDExtension;

public static unsafe class GDExtensionMain
{
	// GDExtensionClassLibraryPtr
	
	// public static GDExtensionInterface extensionInterface;
    public static bool initialized = false;

    
	public static void Initialize()
	{
		
		// Console.WriteLine("Attach Debugger");
		// while (!Debugger.IsAttached)
		// {
		// }
		if (initialized) return;
		
		
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