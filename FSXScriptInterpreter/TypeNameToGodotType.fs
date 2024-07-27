namespace Godot.FSharp

open Godot
open Microsoft.FSharp.Core

module getTypeNameFromIdent =
    open System
    open FSharp.Compiler.Symbols

    type public TypeMatcher(matchAgainst: Godot.VariantType, propertyHint: PropertyHint, hintString: string) =
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

    let VariantMap : Map<string, VariantType> =
        VariantType.GetValues()
        |> Array.map (fun x -> (x.ToString(), x))
        |> Map.ofSeq
    
    let rec convertFSharpTypeToVariantType (typeToConvert: FSharpType) =
        let typeToConvert = typeToConvert.StripAbbreviations()
        let typeDefinition = typeToConvert.TypeDefinition
        let fullName = typeDefinition.FullName

        [ TypeMatcher(VariantType.Bool, PropertyHint.None, "")
            .Add<bool>()

          TypeMatcher(VariantType.Int, PropertyHint.None, "")
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
          TypeMatcher(VariantType.Float, PropertyHint.None, "")
          |> add<float> ()
          |> add<double> ()
          TypeMatcher(VariantType.String, PropertyHint.None, "")
          |> add<string> () ]
        |> tryAndMatch fullName
        |> Option.orElseWith
            (fun () ->
                if typeDefinition.IsEnum then
                    (VariantType.Int,
                     PropertyHint.Enum,
                     String.Join(
                         ",",
                         typeDefinition.FSharpFields
                         |> Seq.map (fun x -> x.Name)
                     ))
                    |> Some
                    |> Some
                else
                    None)
        |> Option.orElseWith
            (fun () ->
                if typeDefinition.IsValueType
                   && typeDefinition.Assembly.SimpleName.Contains("Godot.Bindings")
                   && typeDefinition.Namespace
                      |> Option.map (_.Contains("Godot"))
                      |> Option.defaultValue false then
                    match typeDefinition.FullName with
                    | "Godot.Vector2" -> Some(VariantType.Vector2, PropertyHint.None, "")
                    | "Godot.Vector2I" -> Some(VariantType.Vector2I, PropertyHint.None, "")
                    | "Godot.Rect2" -> Some(VariantType.Rect2, PropertyHint.None, "")
                    | "Godot.Rect2I" -> Some(VariantType.Rect2I, PropertyHint.None, "")
                    | "Godot.Transform2D" -> Some(VariantType.Transform2D, PropertyHint.None, "")
                    | "Godot.Vector3" -> Some(VariantType.Vector3, PropertyHint.None, "")
                    | "Godot.Vector3I" -> Some(VariantType.Vector3I, PropertyHint.None, "")
                    | "Godot.Basis" -> Some(VariantType.Basis, PropertyHint.None, "")
                    | "Godot.Quaternion" -> Some(VariantType.Quaternion, PropertyHint.None, "")
                    | "Godot.Transform3D" -> Some(VariantType.Transform3D, PropertyHint.None, "")
                    | "Godot.Vector4" -> Some(VariantType.Vector4, PropertyHint.None, "")
                    | "Godot.Vector4I" -> Some(VariantType.Vector4I, PropertyHint.None, "")
                    | "Godot.Projection" -> Some(VariantType.Projection, PropertyHint.None, "")
                    | "Godot.Aabb" -> Some(VariantType.Aabb, PropertyHint.None, "")
                    | "Godot.Color" -> Some(VariantType.Color, PropertyHint.None, "")
                    | "Godot.Plane" -> Some(VariantType.Plane, PropertyHint.None, "")
                    | "Godot.Rid" -> Some(VariantType.Rid, PropertyHint.None, "")
                    | "Godot.Callable" -> Some(VariantType.Callable, PropertyHint.None, "")
                    | "Godot.Signal" -> Some(VariantType.Signal, PropertyHint.None, "")
                    | "Godot.StringName" -> Some(VariantType.StringName, PropertyHint.None, "")
                    | "Godot.Variant" -> Some(VariantType.Nil, PropertyHint.None, "")
                    | _ -> None
                    |> Some
                else
                    None)
        |> Option.orElseWith
            (fun () ->
                if typeDefinition.IsArrayType then
                    if typeDefinition.ArrayRank <> 1 then
                        Some None
                    else
                        let generic =
                            typeDefinition.GenericParameters |> Seq.head

                        [ TypeMatcher(VariantType.PackedByteArray, PropertyHint.ArrayType, "")
                            .Add<byte>()
                          TypeMatcher(VariantType.PackedInt32Array, PropertyHint.ArrayType, "")
                              .Add<int32>()
                          TypeMatcher(VariantType.PackedInt64Array, PropertyHint.ArrayType, "")
                              .Add<int64>()
                          TypeMatcher(VariantType.PackedFloat32Array, PropertyHint.ArrayType, "")
                              .Add<float32>()
                          TypeMatcher(VariantType.PackedFloat64Array, PropertyHint.ArrayType, "")
                              .Add<float>()
                          TypeMatcher(VariantType.PackedStringArray, PropertyHint.ArrayType, "")
                              .Add<string>() ]
                        |> tryAndMatch generic.FullName
                        |> Option.orElseWith
                            (fun () ->
                                None)
                else
                    let isGodotArray (typeToCheck: FSharpType) =
                        let typeToCheck = typeToCheck.StripAbbreviations()

                        // GodotArray is sealed
                        $"{typeToCheck.TypeDefinition.AccessPath}.{typeToCheck.TypeDefinition.DisplayName}" = "Godot.Collections.GodotArray"
                    let isPackedGodotArray (typeToCheck: FSharpType) =
                        let typeToCheck = typeToCheck.StripAbbreviations()

                        // PackedArrays are sealed
                        $"{typeToCheck.TypeDefinition.AccessPath}.{typeToCheck.TypeDefinition.DisplayName}".StartsWith "Godot.Collections.Packed"
                    let rec isGodotDictionary (typeToCheck: FSharpType) =
                        let typeToCheck = typeToCheck.StripAbbreviations()

                        // GodotDictionary is sealed
                        $"{typeToCheck.TypeDefinition.AccessPath}.{typeToCheck.TypeDefinition.DisplayName}" = "Godot.Collections.GodotDictionary"
                    let rec isGodotResource (typeToCheck: FSharpType) =
                        let typeToCheck = typeToCheck.StripAbbreviations()

                        if $"{typeToCheck.TypeDefinition.AccessPath}.{typeToCheck.TypeDefinition.DisplayName}" = "Godot.Resource" then
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

                        if $"{typeToCheck.TypeDefinition.AccessPath}.{typeToCheck.TypeDefinition.DisplayName}" = "Godot.GodotObject" then
                            true
                        else
                            match typeToCheck.BaseType with
                            | None -> false
                            | Some value -> isGodotObject <| value.StripAbbreviations()

                    let rec isGodotNode (typeToCheck: FSharpType) =
                        let typeToCheck = typeToCheck.StripAbbreviations()

                        if $"{typeToCheck.TypeDefinition.AccessPath}.{typeToCheck.TypeDefinition.DisplayName}" = "Godot.Node" then
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
                            let genericArgument =
                                typeToConvert
                                    .GenericArguments[ 0 ]
                                    .StripAbbreviations()

                            match convertFSharpTypeToVariantType genericArgument with
                            | None ->
                                Some(VariantType.Array, PropertyHint.None, "")
                                |> Some
                            | Some value ->
                                match value with
                                | None ->
                                    Some(VariantType.Array, PropertyHint.None, "")
                                    |> Some
                                | Some (genericType, propertyHint, hintString) ->
                                    if isGodotNode genericArgument
                                       || isGodotResource genericArgument then
                                        Some(
                                            VariantType.Array,
                                            PropertyHint.TypeString,
                                            $"{genericType |> int}/{propertyHint |> int}:{genericArgument.TypeDefinition.DisplayName}"
                                        )
                                        |> Some
                                    else
                                        Some(VariantType.Array, PropertyHint.TypeString, $"{genericType |> int}/0:")
                                        |> Some
                        else
                            Some(VariantType.Array, PropertyHint.None, "")
                            |> Some
                    elif isPackedGodotArray typeToConvert then
                            let variantType = VariantMap[typeToConvert.TypeDefinition.DisplayName]
                            Some(variantType, PropertyHint.None, "")
                            |> Some
                    elif isGodotDictionary typeToConvert then
                            // Godot Dictionaries don't support typehints yet: https://github.com/godotengine/godot/pull/78656
                            Some(VariantType.Dictionary, PropertyHint.None, "")
                            |> Some
                    elif isStringName typeToConvert then
                        Some(VariantType.StringName, PropertyHint.None, "")
                        |> Some
                    else
                        let isObject, propertyHint = isGodotObject typeToConvert

                        let hintString =
                            match propertyHint with
                            | PropertyHint.ResourceType -> typeDefinition.DisplayName
                            | _ -> ""

                        if isObject then
                            Some(VariantType.Object, propertyHint, hintString)
                            |> Some
                        else
                            None)
