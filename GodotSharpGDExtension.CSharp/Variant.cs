namespace GodotSharpGDExtension;

public partial class Variant : GodotType<Variant>
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
