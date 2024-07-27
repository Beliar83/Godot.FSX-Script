namespace Godot.FSharp


module Variant =

    type DefaultValue =
        | Defined of string
        | Object

    let getGodotDefault (variantType: Godot.VariantType) =
        match variantType with
        | Godot.VariantType.Nil -> DefaultValue.Defined "null"
        | Godot.VariantType.Int -> DefaultValue.Defined "0"
        | Godot.VariantType.Float -> DefaultValue.Defined "0.0"
        | Godot.VariantType.Bool -> DefaultValue.Defined "false"
        | Godot.VariantType.String -> DefaultValue.Defined "String.Empty"
        | Godot.VariantType.Dictionary -> DefaultValue.Defined "new Godot.Dictionary()"
        | Godot.VariantType.Array -> DefaultValue.Defined "new Godot.Array()"
        | Godot.VariantType.Object -> DefaultValue.Object
        | _ -> DefaultValue.Defined $"new {variantType.ToString()}()"
