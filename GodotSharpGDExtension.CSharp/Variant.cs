namespace GodotSharpGDExtension;

public partial class Variant : TypedGodotType<Variant>
{
    public Variant(IntPtr pointer)
    {
        InternalPointer = pointer;
    }
}

public class Variant<T> : Variant
    where T: Variant<T>
{
    /// <inheritdoc />
    public Variant(IntPtr pointer) : base(pointer)
    { }
}
