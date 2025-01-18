namespace Godot.FSharp

open GodotStubs

module Variant =

    type DefaultValue =
        | Simple of string
        | Nil
        | Int
        | Float
        | Object

    let getGodotDefaultForGodotSharp (printError: string -> unit) (variantType: VariantType) =
        match variantType with
        | VariantType.Nil -> DefaultValue.Nil
        | VariantType.Int -> DefaultValue.Int
        | VariantType.Float -> DefaultValue.Float
        | VariantType.Bool -> DefaultValue.Simple "false"
        | VariantType.String -> DefaultValue.Simple "\"\""
        | VariantType.Dictionary -> DefaultValue.Simple "new Godot.Dictionary()"
        | VariantType.Array -> DefaultValue.Simple "new Godot.Array()"
        | VariantType.Object -> DefaultValue.Object
        | _ ->
            printError $"getGodotDefaultForGodotSharp: Unknown VariantType {variantType}"
            DefaultValue.Nil

    type ConversionFunction =
        | Simple of string
        | Nil
        | Int
        | Float
        | Object

    let getConversionToDotnetForGodotSharp (printError: string -> unit) (variantType: VariantType) =
        match variantType with
        | VariantType.Nil -> ConversionFunction.Nil
        | VariantType.Bool -> ConversionFunction.Simple "AsBool"
        | VariantType.Int -> ConversionFunction.Int
        | VariantType.Float -> ConversionFunction.Float
        | VariantType.String -> ConversionFunction.Simple "AsString"
        | VariantType.Vector2 -> ConversionFunction.Simple "AsVector2"
        | VariantType.Vector2I -> ConversionFunction.Simple "AsVector2I"
        | VariantType.Vector3 -> ConversionFunction.Simple "AsVector3"
        | VariantType.Vector3I -> ConversionFunction.Simple "AsVector3I"
        | VariantType.Vector4 -> ConversionFunction.Simple "AsVector4"
        | VariantType.Vector4I -> ConversionFunction.Simple "AsVector4I"
        | VariantType.Rect2 -> ConversionFunction.Simple "AsRect2"
        | VariantType.Rect2I -> ConversionFunction.Simple "AsRect2I"
        | VariantType.Transform2D -> ConversionFunction.Simple "AsTransform2D"
        | VariantType.Transform3D -> ConversionFunction.Simple "AsTransform3D"
        | VariantType.Plane -> ConversionFunction.Simple "AsPlane"
        | VariantType.Quaternion -> ConversionFunction.Simple "AsQuaternion"
        | VariantType.Aabb -> ConversionFunction.Simple "AsAabb"
        | VariantType.Basis -> ConversionFunction.Simple "AsBasis"
        | VariantType.Projection -> ConversionFunction.Simple "AsProjection"
        | VariantType.Color -> ConversionFunction.Simple "AsColor"
        | VariantType.StringName -> ConversionFunction.Simple "AsStringName"
        | VariantType.NodePath -> ConversionFunction.Simple "AsNodePath"
        | VariantType.Rid -> ConversionFunction.Simple "AsRid"
        | VariantType.Object -> ConversionFunction.Object
        | VariantType.Callable -> ConversionFunction.Simple "AsCallable"
        | VariantType.Signal -> ConversionFunction.Simple "AsSignal"
        | VariantType.Dictionary -> ConversionFunction.Simple "AsGodotDictionary"
        | VariantType.Array -> ConversionFunction.Simple "AsGodotArray"
        | VariantType.PackedByteArray -> ConversionFunction.Simple "AsByteArray"
        | VariantType.PackedInt32Array -> ConversionFunction.Simple "AsInt32Array"
        | VariantType.PackedInt64Array -> ConversionFunction.Simple "AsInt64Array"
        | VariantType.PackedFloat32Array -> ConversionFunction.Simple "AsFloat32Array"
        | VariantType.PackedFloat64Array -> ConversionFunction.Simple "AsFloat64Array"
        | VariantType.PackedStringArray -> ConversionFunction.Simple "AsStringArray"
        | VariantType.PackedVector2Array -> ConversionFunction.Simple "AsVector2Array"
        | VariantType.PackedVector3Array -> ConversionFunction.Simple "AsVector3Array"
        | VariantType.PackedColorArray -> ConversionFunction.Simple "AsColorArray"
        | VariantType.PackedVector4Array -> ConversionFunction.Simple "AsVector4Array"
        | _ ->
            printError $"getConversionToDotnetForGodotSharp: Unknown VariantType {variantType}"
            ConversionFunction.Nil
