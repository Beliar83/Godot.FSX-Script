using SharpGen.Runtime;

namespace GodotSharpGDExtension;

public unsafe partial class StringName {

	public static implicit operator StringName(string text) => new StringName(text);

	public static implicit operator string(StringName from) {
		var constructor = GDExtensionInterface.VariantGetPtrConstructor((GDExtensionVariantType)Variant.Type.String, 2);
		var args = stackalloc System.IntPtr[1];
		args[0] = from.internalPointer;
		IntPtr res = IntPtr.Zero;
		GDExtensionInterface.CallGDExtensionPtrConstructor(constructor, res, *args);
		return StringMarshall.ToManaged(res);
	}
}
