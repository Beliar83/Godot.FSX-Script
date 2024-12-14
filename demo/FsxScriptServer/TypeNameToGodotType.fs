namespace Godot.FSharp

open Godot
open Microsoft.FSharp.Core

module getTypeNameFromIdent =
    open System
    open FSharp.Compiler.Symbols

    type public TypeMatcher(matchAgainst: Variant.Type, propertyHint: PropertyHint, hintString: string) =
        let matchesAgainst = ResizeArray()

        member this.Add<'a>() =
            matchesAgainst.Add typeof<'a>.FullName
            this

        member _.Matches comp =
            matchesAgainst |> Seq.exists (fun x -> x = comp)

        member this.TryGetType comp =
            match this.Matches comp with
            | true -> Some(matchAgainst, propertyHint, hintString)
            | false -> None

    let private add<'a> () (on: TypeMatcher) = on.Add<'a>()

    let tryAndMatch fullName =
        Seq.choose (fun (matcher: TypeMatcher) -> fullName |> matcher.TryGetType |> Option.map Some)
        >> Seq.tryHead
    //the below code is pretty much just a direct translation of the existing code
    //for this from the source generators
    //
    //Original code: https://github.com/godotengine/godot/blob/21d080ead4ff09a0796574c920a76e66e8b8a3e4/modules/mono/editor/Godot.NET.Sdk/Godot.SourceGenerators/MarshalUtils.cs#LL86C46-L86C46

    let VariantMap: Map<string, Variant.Type> =
        Variant.Type.GetValues() |> Array.map (fun x -> (x.ToString(), x)) |> Map.ofSeq

    let rec convertFSharpTypeToVariantType (typeToConvert: FSharpType) =
        let typeToConvert = typeToConvert.StripAbbreviations()
        let typeDefinition = typeToConvert.TypeDefinition

        let fullName =
            if typeDefinition.IsArrayType then
                "System.Array"
            else
                typeDefinition.FullName

        [ TypeMatcher(Variant.Type.Bool, PropertyHint.None, "").Add<bool>()

          TypeMatcher(Variant.Type.Int, PropertyHint.None, "")
          |> add<char> ()
          |> add<sbyte> ()
          |> add<int16> ()
          |> add<int32> ()
          |> add<int64> ()
          |> add<int8> ()
          |> add<uint> ()
          |> add<uint16> ()
          |> add<uint32> ()
          |> add<uint64> ()
          |> add<uint16> ()
          TypeMatcher(Variant.Type.Float, PropertyHint.None, "")
          |> add<float> ()
          |> add<double> ()
          TypeMatcher(Variant.Type.String, PropertyHint.None, "") |> add<string> () ]
        |> tryAndMatch fullName
        |> Option.orElseWith (fun () ->
            if typeDefinition.IsEnum then
                (Variant.Type.Int,
                 PropertyHint.Enum,
                 String.Join(",", typeDefinition.FSharpFields |> Seq.map (fun x -> x.Name)))
                |> Some
                |> Some
            else
                None)
        |> Option.orElseWith (fun () ->
            if
                (typeDefinition.Assembly.SimpleName.Contains("Godot.Bindings")
                 || typeDefinition.Assembly.SimpleName.Contains("GodotSharp"))
                && typeDefinition.Namespace
                   |> Option.map (_.Contains("Godot"))
                   |> Option.defaultValue false
            then
                match typeDefinition.FullName with
                | "Godot.Vector2" -> Some(Variant.Type.Vector2, PropertyHint.None, "")
                | "Godot.Vector2I" -> Some(Variant.Type.Vector2I, PropertyHint.None, "")
                | "Godot.Rect2" -> Some(Variant.Type.Rect2, PropertyHint.None, "")
                | "Godot.Rect2I" -> Some(Variant.Type.Rect2I, PropertyHint.None, "")
                | "Godot.Transform2D" -> Some(Variant.Type.Transform2D, PropertyHint.None, "")
                | "Godot.Vector3" -> Some(Variant.Type.Vector3, PropertyHint.None, "")
                | "Godot.Vector3I" -> Some(Variant.Type.Vector3I, PropertyHint.None, "")
                | "Godot.Basis" -> Some(Variant.Type.Basis, PropertyHint.None, "")
                | "Godot.Quaternion" -> Some(Variant.Type.Quaternion, PropertyHint.None, "")
                | "Godot.Transform3D" -> Some(Variant.Type.Transform3D, PropertyHint.None, "")
                | "Godot.Vector4" -> Some(Variant.Type.Vector4, PropertyHint.None, "")
                | "Godot.Vector4I" -> Some(Variant.Type.Vector4I, PropertyHint.None, "")
                | "Godot.Projection" -> Some(Variant.Type.Projection, PropertyHint.None, "")
                | "Godot.Aabb" -> Some(Variant.Type.Aabb, PropertyHint.None, "")
                | "Godot.Color" -> Some(Variant.Type.Color, PropertyHint.None, "")
                | "Godot.Plane" -> Some(Variant.Type.Plane, PropertyHint.None, "")
                | "Godot.Rid" -> Some(Variant.Type.Rid, PropertyHint.None, "")
                | "Godot.Callable" -> Some(Variant.Type.Callable, PropertyHint.None, "")
                | "Godot.Signal" -> Some(Variant.Type.Signal, PropertyHint.None, "")
                | "Godot.StringName" -> Some(Variant.Type.StringName, PropertyHint.None, "")
                | "Godot.NodePath" -> Some(Variant.Type.NodePath, PropertyHint.None, "")
                | "Godot.Variant" -> Some(Variant.Type.Nil, PropertyHint.None, "")
                | _ -> None
                |> Some
            else
                None)
        |> Option.orElseWith (fun () ->
            if typeDefinition.IsArrayType then
                if typeDefinition.ArrayRank <> 1 then
                    Some None
                else
                    let generic = typeDefinition.GenericArguments |> Seq.head

                    let generic =
                        if generic.IsAbbreviation then
                            generic.AbbreviatedType.TypeDefinition
                        else
                            generic.TypeDefinition

                    [ TypeMatcher(Variant.Type.PackedByteArray, PropertyHint.ArrayType, "")
                          .Add<byte>()
                      TypeMatcher(Variant.Type.PackedInt32Array, PropertyHint.ArrayType, "")
                          .Add<int32>()
                      TypeMatcher(Variant.Type.PackedInt64Array, PropertyHint.ArrayType, "")
                          .Add<int64>()
                      TypeMatcher(Variant.Type.PackedFloat32Array, PropertyHint.ArrayType, "")
                          .Add<float32>()
                      TypeMatcher(Variant.Type.PackedFloat64Array, PropertyHint.ArrayType, "")
                          .Add<float>()
                      TypeMatcher(Variant.Type.PackedStringArray, PropertyHint.ArrayType, "")
                          .Add<string>() ]
                    |> tryAndMatch generic.FullName
                    |> Option.orElseWith (fun () ->
                        if
                            generic.IsValueType
                            && (generic.Assembly.SimpleName.Contains("Godot.Bindings")
                                || generic.Assembly.SimpleName.Contains("GodotSharp"))
                            && generic.Namespace
                               |> Option.map (_.Contains("Godot"))
                               |> Option.defaultValue false
                        then
                            match generic.FullName with
                            | "Godot.Vector2" -> Some(Variant.Type.PackedVector2Array, PropertyHint.None, "")
                            | "Godot.Vector3" -> Some(Variant.Type.PackedVector3Array, PropertyHint.None, "")
                            | "Godot.Vector4" -> Some(Variant.Type.PackedVector4Array, PropertyHint.None, "")
                            | "Godot.Color" -> Some(Variant.Type.PackedColorArray, PropertyHint.None, "")
                            | _ -> None
                            |> Some
                        else
                            None)
            else
                let isGodotArray (typeToCheck: FSharpType) =
                    let typeToCheck = typeToCheck.StripAbbreviations()
                    // GodotArray is sealed
                    $"{typeToCheck.TypeDefinition.AccessPath}.{typeToCheck.TypeDefinition.DisplayName}" = "Godot.Collections.GodotArray"
                    || $"{typeToCheck.TypeDefinition.AccessPath}.{typeToCheck.TypeDefinition.DisplayName}" = "Godot.Collections.Array"

                let isPackedGodotArray (typeToCheck: FSharpType) =
                    let typeToCheck = typeToCheck.StripAbbreviations()

                    // PackedArrays are sealed
                    $"{typeToCheck.TypeDefinition.AccessPath}.{typeToCheck.TypeDefinition.DisplayName}"
                        .StartsWith
                        "Godot.Collections.Packed"

                let rec isGodotDictionary (typeToCheck: FSharpType) =
                    let typeToCheck = typeToCheck.StripAbbreviations()

                    // GodotDictionary is sealed
                    $"{typeToCheck.TypeDefinition.AccessPath}.{typeToCheck.TypeDefinition.DisplayName}" = "Godot.Collections.GodotDictionary"
                    || $"{typeToCheck.TypeDefinition.AccessPath}.{typeToCheck.TypeDefinition.DisplayName}" = "Godot.Collections.Dictionary"

                let rec isGodotResource (typeToCheck: FSharpType) =
                    let typeToCheck = typeToCheck.StripAbbreviations()

                    if
                        $"{typeToCheck.TypeDefinition.AccessPath}.{typeToCheck.TypeDefinition.DisplayName}" = "Godot.Resource"
                    then
                        true
                    else
                        match typeToCheck.BaseType with
                        | None -> false
                        | Some value -> isGodotResource <| value.StripAbbreviations()

                let isStringName (typeToCheck: FSharpType) =
                    let typeToCheck = typeToCheck.StripAbbreviations()

                    // StringName is sealed
                    $"{typeToCheck.TypeDefinition.AccessPath}.{typeToCheck.TypeDefinition.DisplayName}" = "Godot.StringName"


                let rec isGodotObject (typeToCheck: FSharpType) =
                    let typeToCheck = typeToCheck.StripAbbreviations()

                    if
                        $"{typeToCheck.TypeDefinition.AccessPath}.{typeToCheck.TypeDefinition.DisplayName}" = "Godot.GodotObject"
                    then
                        true
                    else
                        match typeToCheck.BaseType with
                        | None -> false
                        | Some value -> isGodotObject <| value.StripAbbreviations()

                let rec isGodotNode (typeToCheck: FSharpType) =
                    let typeToCheck = typeToCheck.StripAbbreviations()

                    if
                        $"{typeToCheck.TypeDefinition.AccessPath}.{typeToCheck.TypeDefinition.DisplayName}" = "Godot.Node"
                    then
                        true
                    else
                        match typeToCheck.BaseType with
                        | None -> false
                        | Some value -> isGodotNode <| value.StripAbbreviations()

                let isGodotObject (entity: FSharpType) =
                    if isGodotResource entity then
                        (true, PropertyHint.ResourceType)
                    elif isGodotNode entity then
                        (true, PropertyHint.NodeType)
                    elif isGodotObject entity then
                        (true, PropertyHint.None)
                    else
                        (false, PropertyHint.None)

                if isGodotArray typeToConvert then
                    if typeToConvert.GenericArguments.Count = 1 then
                        let genericArgument = typeToConvert.GenericArguments[0].StripAbbreviations()

                        match convertFSharpTypeToVariantType genericArgument with
                        | None -> Some(Variant.Type.Array, PropertyHint.None, "") |> Some
                        | Some value ->
                            match value with
                            | None -> Some(Variant.Type.Array, PropertyHint.None, "") |> Some
                            | Some(genericType, propertyHint, hintString) ->
                                if isGodotNode genericArgument || isGodotResource genericArgument then
                                    Some(
                                        Variant.Type.Array,
                                        PropertyHint.TypeString,
                                        $"{genericType |> int}/{propertyHint |> int}:{genericArgument.TypeDefinition.DisplayName}"
                                    )
                                    |> Some
                                else
                                    Some(Variant.Type.Array, PropertyHint.TypeString, $"{genericType |> int}/0:")
                                    |> Some
                    else
                        Some(Variant.Type.Array, PropertyHint.None, "") |> Some
                elif isPackedGodotArray typeToConvert then
                    let VariantType = VariantMap[typeToConvert.TypeDefinition.DisplayName]
                    Some(VariantType, PropertyHint.None, "") |> Some
                elif isGodotDictionary typeToConvert then
                    // Godot Dictionaries don't support typehints yet: https://github.com/godotengine/godot/pull/78656 // TODO: GodotSharp does now
                    Some(Variant.Type.Dictionary, PropertyHint.None, "") |> Some
                elif isStringName typeToConvert then
                    Some(Variant.Type.StringName, PropertyHint.None, "") |> Some
                else
                    let isObject, propertyHint = isGodotObject typeToConvert

                    let hintString =
                        match propertyHint with
                        | PropertyHint.ResourceType -> typeDefinition.DisplayName
                        | _ -> ""

                    if isObject then
                        Some(Variant.Type.Object, propertyHint, hintString) |> Some
                    else
                        None)
