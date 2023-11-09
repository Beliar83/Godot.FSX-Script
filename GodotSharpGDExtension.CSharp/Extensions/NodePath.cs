namespace GodotSharpGDExtension;

public unsafe partial class NodePath {

	public static implicit operator NodePath(string text) => new NodePath(text);

	public static implicit operator string(NodePath from) {
		var constructor = GDExtensionMain.extensionInterface.variant_get_ptr_constructor.Data.Pointer((GDExtensionInterface.GDExtensionVariantType)Variant.Type.String, 3);
		var args = stackalloc GDExtensionInterface.GDExtensionConstTypePtr[1];
		args[0] = from._internal_pointer;
		void* res;
		constructor.Data.Pointer(&res, args);
		return StringMarshall.ToManaged(res);
	}
}
