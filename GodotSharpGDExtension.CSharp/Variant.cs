namespace GodotSharpGDExtension;

public unsafe class Variant
{
    public __GdextType* InternalPointer { get; set; }
    
    public Variant(__GdextType* pointer)
    {
        InternalPointer = pointer;
    }

    public static implicit operator __GdextType*(Variant from)
    {
        return from.InternalPointer;
    }
}

public class Variant<T> : Variant
    where T: Variant<T>
{
    /// <inheritdoc />
    public unsafe Variant(__GdextType* pointer) : base(pointer)
    { }    

}
