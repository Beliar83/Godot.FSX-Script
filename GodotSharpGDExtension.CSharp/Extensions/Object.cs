using System.Diagnostics;

namespace GodotSharpGDExtension;

public unsafe partial class Object : Wrapped {
	public static Object ConstructUnknown(IntPtr ptr) {
		if (ptr == IntPtr.Zero) { return null!; }
		var o = new Object(ptr);
		string c = o.GetClass();
		return constructors[c](ptr);
	}

	public delegate Object Constructor(IntPtr data);
	public static bool RegisterConstructor(string name, Constructor constructor) => constructors.TryAdd(name, constructor);

	static System.Collections.Generic.Dictionary<string, Constructor> constructors = new();
}
