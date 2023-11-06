using System.Diagnostics;

namespace GodotSharpGDExtension;

public unsafe partial class GodotObject : Wrapped {
	public static GodotObject ConstructUnknown(IntPtr ptr) {
		if (ptr == IntPtr.Zero) { return null!; }
		var o = new GodotObject(ptr);
		string c = o.GetClass();
		return constructors[c](ptr);
	}

	public delegate GodotObject Constructor(IntPtr data);
	public static bool RegisterConstructor(string name, Constructor constructor) => constructors.TryAdd(name, constructor);

	static System.Collections.Generic.Dictionary<string, Constructor> constructors = new();
}
