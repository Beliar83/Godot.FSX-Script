namespace GodotSharpGDExtension;

public unsafe partial class StringName {

	public static implicit operator StringName(string text) => new StringName(text);

	public static implicit operator string(StringName from) {
		var constructor = GDExtensionMain.extensionInterface.variant_get_ptr_constructor.Data.Pointer((GDExtensionInterface.GDExtensionVariantType)Variant.Type.String, 2);
		var args = stackalloc GDExtensionInterface.GDExtensionConstTypePtr[1];
		args[0] = from._internal_pointer;
		void* res;
		constructor.Data.Pointer(&res, args);
		return StringMarshall.ToManaged(&res);
	}
}
