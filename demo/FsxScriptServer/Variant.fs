namespace Godot.FSharp

open Godot


module Variant =

    type DefaultValue =
        | Simple of string
        | Nil
        | Int
        | Float
        | Object

    let getGodotDefaultForGodotSharp (variantType: Variant.Type) =
        match variantType with
        | Variant.Type.Nil -> DefaultValue.Nil
        | Variant.Type.Int -> DefaultValue.Int
        | Variant.Type.Float -> DefaultValue.Float
        | Variant.Type.Bool -> DefaultValue.Simple "false"
        | Variant.Type.String -> DefaultValue.Simple "String.Empty"
        | Variant.Type.Dictionary -> DefaultValue.Simple "new Godot.Dictionary()"
        | Variant.Type.Array -> DefaultValue.Simple "new Godot.Array()"
        | Variant.Type.Object -> DefaultValue.Object
        | _ ->
            GD.PrintErr $"getGodotDefaultForGodotSharp: Unknown VariantType {variantType}"
            DefaultValue.Nil

    type ConversionFunction =
        | Simple of string
        | Nil
        | Int
        | Float
        | Object

    let getConversionToDotnetForGodotSharp (variantType: Variant.Type) =
        match variantType with
        | Variant.Type.Nil -> ConversionFunction.Nil
        | Variant.Type.Bool -> ConversionFunction.Simple "AsBool"
        | Variant.Type.Int -> ConversionFunction.Int
        | Variant.Type.Float -> ConversionFunction.Float
        | Variant.Type.String -> ConversionFunction.Simple "AsString"
        | Variant.Type.Vector2 -> ConversionFunction.Simple "AsVector2"
        | Variant.Type.Vector2I -> ConversionFunction.Simple "AsVector2I"
        | Variant.Type.Vector3 -> ConversionFunction.Simple "AsVector3"
        | Variant.Type.Vector3I -> ConversionFunction.Simple "AsVector3I"
        | Variant.Type.Vector4 -> ConversionFunction.Simple "AsVector4"
        | Variant.Type.Vector4I -> ConversionFunction.Simple "AsVector4I"
        | Variant.Type.Rect2 -> ConversionFunction.Simple "AsRect2"
        | Variant.Type.Rect2I -> ConversionFunction.Simple "AsRect2I"
        | Variant.Type.Transform2D -> ConversionFunction.Simple "AsTransform2D"
        | Variant.Type.Transform3D -> ConversionFunction.Simple "AsTransform3D"
        | Variant.Type.Plane -> ConversionFunction.Simple "AsPlane"
        | Variant.Type.Quaternion -> ConversionFunction.Simple "AsQuaternion"
        | Variant.Type.Aabb -> ConversionFunction.Simple "AsAabb"
        | Variant.Type.Basis -> ConversionFunction.Simple "AsBasis"
        | Variant.Type.Projection -> ConversionFunction.Simple "AsProjection"
        | Variant.Type.Color -> ConversionFunction.Simple "AsColor"
        | Variant.Type.StringName -> ConversionFunction.Simple "AsStringName"
        | Variant.Type.NodePath -> ConversionFunction.Simple "AsNodePath"
        | Variant.Type.Rid -> ConversionFunction.Simple "AsRid"
        | Variant.Type.Object -> ConversionFunction.Object
        | Variant.Type.Callable -> ConversionFunction.Simple "AsCallable"
        | Variant.Type.Signal -> ConversionFunction.Simple "AsSignal"
        | Variant.Type.Dictionary -> ConversionFunction.Simple "AsGodotDictionary"
        | Variant.Type.Array -> ConversionFunction.Simple "AsGodotArray"
        | Variant.Type.PackedByteArray -> ConversionFunction.Simple "AsByteArray"
        | Variant.Type.PackedInt32Array -> ConversionFunction.Simple "AsInt32Array"
        | Variant.Type.PackedInt64Array -> ConversionFunction.Simple "AsInt64Array"
        | Variant.Type.PackedFloat32Array -> ConversionFunction.Simple "AsFloat32Array"
        | Variant.Type.PackedFloat64Array -> ConversionFunction.Simple "AsFloat64Array"
        | Variant.Type.PackedStringArray -> ConversionFunction.Simple "AsStringArray"
        | Variant.Type.PackedVector2Array -> ConversionFunction.Simple "AsVector2Array"
        | Variant.Type.PackedVector3Array -> ConversionFunction.Simple "AsVector3Array"
        | Variant.Type.PackedColorArray -> ConversionFunction.Simple "AsColorArray"
        | Variant.Type.PackedVector4Array -> ConversionFunction.Simple "AsVector4Array"
        | _ ->
            GD.PrintErr $"getConversionToDotnetForGodotSharp: Unknown VariantType {variantType}"
            ConversionFunction.Nil
