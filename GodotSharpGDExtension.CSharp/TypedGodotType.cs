// using System.Runtime.CompilerServices;
// using GodotSharpGDExtension;
//
// namespace GodotSharpGDExtension;
//
// // This is generic purely so IDEs do not suggest changing argument types to GodotType  
// internal interface IGodotType
// {
//     public GodotType InternalPointer { get; }
// }
//
// internal abstract class TypedGodotType<T> : IGodotType
//     where T: TypedGodotType<T>
// {
//     public GodotType InternalPointer { get; protected init; }
//
//     public static implicit operator GodotType(TypedGodotType<T> value)
//     {
//         return value.InternalPointer;
//     }
// }
