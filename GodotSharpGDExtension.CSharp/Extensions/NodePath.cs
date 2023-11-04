namespace GodotSharpGDExtension;

public unsafe partial class NodePath {

	public static implicit operator NodePath(string text) => new NodePath(text);

	public static implicit operator string(NodePath from) {
		var constructor = GDExtensionInterface.VariantGetPtrConstructor((GDExtensionVariantType)Variant.Type.String, 3);
		var args = stackalloc IntPtr[1];
		args[0] = from.internalPointer;
		IntPtr res = IntPtr.Zero;
		GDExtensionInterface.CallGDExtensionPtrConstructor(constructor, res, *args);
		
		return StringMarshall.ToManaged(res);
	}
}
