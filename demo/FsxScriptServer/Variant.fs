namespace Godot.FSharp

open Godot


module Variant =

    type DefaultValue =
        | Defined of string
        | Object

    let getGodotDefault (variantType: Variant.Type) =
        match variantType with
        | Variant.Type.Nil -> DefaultValue.Defined "null"
        | Variant.Type.Int -> DefaultValue.Defined "0"
        | Variant.Type.Float -> DefaultValue.Defined "0.0"
        | Variant.Type.Bool -> DefaultValue.Defined "false"
        | Variant.Type.String -> DefaultValue.Defined "String.Empty"
        | Variant.Type.Dictionary -> DefaultValue.Defined "new Godot.Dictionary()"
        | Variant.Type.Array -> DefaultValue.Defined "new Godot.Array()"
        | Variant.Type.Object -> DefaultValue.Object
        | _ -> DefaultValue.Defined $"new {variantType.ToString()}()"
