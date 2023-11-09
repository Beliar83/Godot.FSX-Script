using System.Diagnostics;

namespace GodotSharpGDExtension;

public unsafe partial class Object : Wrapped {
	public static Object ConstructUnknown(void* ptr) {
		if (ptr == null) { return null!; }
		var o = new Object(ptr);
		var c = o.GetClass();
		return constructors[c](ptr);
	}

	public delegate Object Constructor(void* data);
	public static bool RegisterConstructor(string name, Constructor constructor) => constructors.TryAdd(name, constructor);

	static System.Collections.Generic.Dictionary<string, Constructor> constructors = new();
}
