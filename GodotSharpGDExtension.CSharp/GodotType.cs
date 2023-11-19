namespace GodotSharpGDExtension;

// This is generic purely so IDEs do not suggest changing argument types to GodotType  
internal interface IGodotType
{
    public IntPtr InternalPointer { get; }
}

public abstract class GodotType<T> : IGodotType
    where T: GodotType<T>
{
    public IntPtr InternalPointer { get; protected init; }
}
