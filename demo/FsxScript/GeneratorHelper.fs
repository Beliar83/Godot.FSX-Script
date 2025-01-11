// Taken and modified from https://github.com/Beliar83/Godot.FSharp. Original authors: lenscas (helpers.fs), Beliar83
namespace Godot.FSharp

open FSharp.Compiler.Symbols
open FSharp.Compiler.Syntax

module GeneratorHelper =
    let defaultPropertyUsage =
        "int(PropertyUsageFlags.Default ||| PropertyUsageFlags.Editor)"

    let rec extractTypes (moduleDecls: SynModuleDecl list) (ns: LongIdent) =
        [ for moduleDecl in moduleDecls do
              match moduleDecl with
              | SynModuleDecl.Types(types, _) -> yield (ns, types)
              | SynModuleDecl.NestedModule(synComponentInfo, _, decls, _, _, _) ->
                  match synComponentInfo with
                  | SynComponentInfo(_, _, _, longId, _, _, _, _) ->
                      let combined = longId |> List.append ns
                      yield! (extractTypes decls combined)
              | other -> yield! extractTypes [ other ] ns ]

    let extractFunctions (moduleDecls: SynModuleDecl list) =
        [ for moduleDecl in moduleDecls do
              match moduleDecl with
              | SynModuleDecl.Let(_, bindings, _) ->
                  bindings
                  |> List.choose (fun x ->
                      let SynBinding(accessibility, _, _, _, _, _, valData, headPat, _, _, _, _, _) as _ =
                          x

                      match (accessibility, valData) with
                      | _, SynValData(_, SynValInfo([], _), _) -> None
                      | (Some(SynAccess.Public _ | SynAccess.Internal _) | None), SynValData(_, SynValInfo(x, _), _) ->
                          Some(valData, headPat, x)
                      | Some(SynAccess.Private _), _ -> None)
              | _ -> () ]


    let rec extractMethods (typ: FSharpType) =
        let typMethods =
            typ.TypeDefinition.MembersFunctionsAndValues
            |> Seq.filter (fun x -> x.IsMethod && not <| x.IsOverrideOrExplicitInterfaceImplementation)
            |> List.ofSeq

        match typ.BaseType with
        | None -> typMethods
        | Some baseType -> typMethods |> List.append (extractMethods baseType)

    let rec extractModules (declarations: FSharpImplementationFileDeclaration list) =
        declarations
        |> List.choose (fun x ->
            match x with
            | FSharpImplementationFileDeclaration.Entity(entity, declarations) -> Some(entity, declarations)
            | _ -> None)
        |> List.collect (fun (x, entityDeclarations) ->
            if x.IsNamespace || x.IsFSharpModule then
                [ (x, entityDeclarations) ] |> List.append (extractModules entityDeclarations)
            else
                [])

    let rec extractRecordsAndUnions (declarations: FSharpImplementationFileDeclaration list) =
        declarations
        |> List.choose (fun x ->
            match x with
            | FSharpImplementationFileDeclaration.Entity(entity, declarations) -> Some(entity, declarations)
            | _ -> None)
        |> List.collect (fun (x, entityDeclarations) ->
            if x.IsNamespace || x.IsFSharpModule then
                extractRecordsAndUnions entityDeclarations
            elif x.IsFSharpRecord || x.IsFSharpUnion then
                [ x ]
            else
                [])

    let getNamespaceOfEntity (definition: FSharpEntity) =
        match definition.DeclaringEntity with
        | None ->
            match definition.Namespace with
            | None -> None
            | Some value -> Some($"{value}")
        | Some declaringEntity ->
            match declaringEntity.Namespace with
            | None -> Some($"{declaringEntity.FullName}")
            | Some entityNamespace -> Some($"{entityNamespace}.{declaringEntity.FullName}")

    let getNamespaceOfType (typ: FSharpType) = getNamespaceOfEntity typ.TypeDefinition

    let getScope (entity: FSharpEntity) =
        match entity.DeclaringEntity with
        | Some declaringEntity ->
            match declaringEntity.Namespace with
            | Some value -> $"{value}.{declaringEntity.DisplayName}"
            | None -> declaringEntity.FullName
        | None ->
            match entity.Namespace with
            | Some value -> value
            | None -> "global"

    let rec getTypeString (fsharpType: FSharpType) : string =
        let typeName =
            match getNamespaceOfType fsharpType with
            | None -> fsharpType.TypeDefinition.DisplayName
            | Some typeNamespace -> $"{typeNamespace}.{fsharpType.TypeDefinition.DisplayName}"

        if fsharpType.GenericArguments.Count > 0 then
            let genericType =
                [ for param in fsharpType.GenericArguments do
                      getTypeString (param.StripAbbreviations()) ]
                |> String.concat ", "

            $"{typeName}<{genericType}>"
        else
            typeName
