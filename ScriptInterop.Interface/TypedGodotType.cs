using System.Runtime.CompilerServices;

namespace GodotSharpGDExtension;

// This is generic purely so IDEs do not suggest changing argument types to GodotType  
public interface IGodotType
{
    public IntPtr InternalPointer { get; }
}

public abstract class TypedGodotType<T> : IGodotType
    where T: TypedGodotType<T>
{
    public IntPtr InternalPointer { get; protected init; }
    

    public static implicit operator GodotType(TypedGodotType<T> value)
    {
        return new GodotType { Pointer = value.InternalPointer};
    }
}
