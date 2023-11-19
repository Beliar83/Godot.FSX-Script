using System.Diagnostics;
using System.Reflection;

namespace GodotSharpGDExtension;

public static unsafe class GDExtensionMain
{
	// public static GDExtensionInterface.interface_functions extensionInterface;
    // public static GDExtensionInterface.GDExtensionClassLibraryPtr library;
    public static bool Initialized = false;
    // public static delegate* unmanaged<GDExtensionInterface.GDExtensionConstTypePtr, void> test1;
    // public static delegate* unmanaged<GDExtensionInterface.GDExtensionObjectPtr, GDExtensionInterface.GDExtensionConstTypePtr, void> test2;


    // [UnmanagedCallersOnly]
    // public static GDExtensionInterface.GDExtensionBool Init(GDExtensionInterface.GDExtensionInterfaceGetProcAddressC2CS p_get_proc_address,
	   //  GDExtensionInterface.GDExtensionClassLibraryPtr p_library,
	   //  GDExtensionInterface.GDExtensionInitialization* r_initialization)
    // {
	   //  return 0;
    // }
    
	// public static void Initialize(GDExtensionInterface.GDExtensionClassLibraryPtr libraryPtr, GDExtensionInterface.interface_functions interfaceFunctions)
	// {
	// 	// Console.WriteLine("Attach Debugger");
	// 	// while (!Debugger.IsAttached)
	// 	// {
	// 	// }
	// 	if (Initialized) return;
	// 	extensionInterface = interfaceFunctions;
 //        library = libraryPtr;
 //        // test1 = t1;
 //        // test2 = t2;
	// }
	
	// public static void* MoveToUnmanaged<T>(T value) where T : unmanaged
	// {
	// 	var ptr = (T*)extensionInterface.mem_alloc.Data.Pointer((nuint)sizeof(T));
	// 	*ptr = value;
	// 	return ptr;
	// }
	
	
}