namespace GodotSharpGDExtension;

public partial class RefCounted {

	~RefCounted() {
		Unreference();
	}
}
