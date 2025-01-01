// Taken and modified from https://github.com/Beliar83/Godot.FSharp. Original authors: lenscas (generator.fs), Beliar83
namespace Godot.FSharp

open System
open FSharp.Compiler.Symbols
open Godot
open FSharp.Compiler.Syntax
open Microsoft.FSharp.Core

type ExtraParamCountCheckMode =
    | ZeroOrMore
    | Exact of int

module ObjectGenerator =
    type NodeScriptAttribute() =
        inherit Attribute()

    type NodeAttribute() =
        inherit Attribute()

    type StateAttribute() =
        inherit Attribute()

    type ExportAttribute() =
        inherit Attribute()

    type NodeOrState =
        | Node of SynComponentInfo
        | State of SynComponentInfo

    type INodeWithState<'Node, 'State> =
        abstract member GetState: unit -> 'State
        abstract member SetState: 'State -> unit
        abstract member GetNode: unit -> 'Node

    type TypeData =
        { Name: string
          OfTypeName: string
          OfType: Variant.Type
          PropertyHint: PropertyHint
          HintText: string
          UsageFlags: PropertyUsageFlags }

    type MethodParam = TypeData

    type MethodsToGenerate =
        { IsOverride: bool
          MethodParams: List<MethodParam>
          IsCurried: bool
          MethodName: string
          MethodFlags: MethodFlags
          ReturnParameter: Option<MethodParam> }

    type Field = TypeData

    type StateToGenerate =
        { Name: string
          ExportedFields: List<Field>
          InnerFields: List<Field> }

    type ToGenerateInfo =
        { ModuleNameToOpen: string
          Extending: StringName
          ExtendingNamespace: string
          Name: StringName
          StateToGenerate: StateToGenerate
          methods: List<MethodsToGenerate> }

    let extractTypesNonRecursive (moduleDecls: FSharpImplementationFileDeclaration list) =
        moduleDecls
        |> List.choose
            (fun x ->
                match x with
                | FSharpImplementationFileDeclaration.Entity (entity, declarations) -> Some(entity, declarations)
                | _ -> None)


    let extractNodeType (typ: FSharpEntity) =
        if typ.DisplayName = "Base" then
            if typ.IsFSharpAbbreviation then
                Some(typ.AbbreviatedType)
            else
                None
        else
            None

    let extractStateType (typ: FSharpEntity) =
        if typ.DisplayName = "State" then
            Some(typ)
        else
            None

    let extractNodeDefinition moduleDecls =
        let entities = extractTypesNonRecursive moduleDecls

        let state =
            entities
            |> List.choose (fun (entity, _) -> extractStateType entity)
            |> List.tryHead

        let node =
            entities
            |> List.choose (fun (entity, _) -> extractNodeType entity)
            |> List.tryHead

        match (state, node) with
        | Some x, Some y ->

            (x, y)
        | None, _ ->
            "Missing state in node module"
            |> Exception
            |> raise
        | _, None ->
            "Missing node in node module"
            |> Exception
            |> raise


    let extractNodes (contents: FSharpImplementationFileContents) =
        GeneratorHelper.extractModules contents.Declarations
        |> List.filter
            (fun (x, _) ->
                x.Attributes
                |> Seq.exists (fun x -> x.IsAttribute<NodeScriptAttribute>()))

    let isValidNodeMethod
        (method: FSharpMemberOrFunctionOrValue)
        (state: FSharpEntity)
        (node: FSharpEntity)
        extraParamCountCheckMode
        =
        let isCurriedMethod = method.CurriedParameterGroups.Count >= 2

        let parameterCountIsValid =
            match extraParamCountCheckMode with
            | ExtraParamCountCheckMode.ZeroOrMore ->
                if isCurriedMethod then
                    method.CurriedParameterGroups.Count >= 2
                else
                    (method.CurriedParameterGroups |> Seq.head).Count >= 2
            | Exact count ->
                if isCurriedMethod then
                    method.CurriedParameterGroups.Count = 2 + count
                else
                    (method.CurriedParameterGroups |> Seq.head).Count = 2 + count

        let nodeArgument =
            (method.CurriedParameterGroups
             |> Seq.head
             |> Seq.head)
                .Type.StripAbbreviations()

        let stateArgument =
            if isCurriedMethod then
                (method.CurriedParameterGroups
                 |> Seq.last
                 |> Seq.head)
                    .Type.StripAbbreviations()
            else
                (method.CurriedParameterGroups
                 |> Seq.head
                 |> Seq.last)
                    .Type.StripAbbreviations()

        if not <| parameterCountIsValid then
            false
        elif isCurriedMethod
             && method.CurriedParameterGroups
                |> Seq.head
                |> Seq.length
                <> 1 then
            false
        elif isCurriedMethod
             && method.CurriedParameterGroups
                |> Seq.last
                |> Seq.length
                <> 1 then
            false
        elif method.CurriedParameterGroups
             |> Seq.collect id
             |> Seq.exists (fun x -> not <| x.Type.HasTypeDefinition) then
            false
        else

            let nodeArgumentTypeDefinition = nodeArgument.TypeDefinition

            let stateArgument = stateArgument.TypeDefinition

            let returnParameterType = method.ReturnParameter.Type

            nodeArgumentTypeDefinition = node
            && stateArgument = state
            && (

            (returnParameterType.IsTupleType
             && returnParameterType.GenericArguments.Count = 2
             && (returnParameterType.GenericArguments |> Seq.head)
                 .StripAbbreviations()
                 .TypeDefinition = state)
            || returnParameterType.TypeDefinition = state)

    let isValidReadySignature (method: FSharpMemberOrFunctionOrValue) (state: FSharpEntity) (node: FSharpEntity) =
        isValidNodeMethod method state node (Exact(0))

    let isValidProcessSignature (method: FSharpMemberOrFunctionOrValue) (state: FSharpEntity) (node: FSharpEntity) =
        if not
           <| isValidNodeMethod method state node (Exact(1)) then
            false
        elif method.CurriedParameterGroups[1].Count <> 1 then
            false
        else
            let deltaArgument =
                (method.CurriedParameterGroups |> Seq.head |> Seq.head)
                    .Type.StripAbbreviations()

            let deltaArgumentTypeDefinition = deltaArgument.TypeDefinition
            deltaArgumentTypeDefinition.FullName = typeof<Double>.FullName

    let isValidGetPropertyListSignature
        (method: FSharpMemberOrFunctionOrValue)
        (state: FSharpEntity)
        (node: FSharpEntity)
        =
        if not
           <| isValidNodeMethod method state node (Exact(0)) then
            false
        elif not <| method.ReturnParameter.Type.IsTupleType then
            false
        elif method.ReturnParameter.Type.GenericArguments.Count
             <> 2 then
            false
        elif (method.ReturnParameter.Type.GenericArguments |> Seq.head)
                 .StripAbbreviations()
                 .TypeDefinition
             <> state then
            false
        else
            let returnType =
                method
                    .ReturnParameter
                    .Type
                    .GenericArguments[ 1 ]
                    .StripAbbreviations()

            if $"{returnType.TypeDefinition.AccessPath}.{returnType.TypeDefinition.DisplayName}"
               <> "Godot.Collections.Array" then
                false
            elif returnType.GenericArguments.Count <> 1 then
                false
            else
                let arrayItemItem =
                    returnType
                        .GenericArguments[ 0 ]
                        .StripAbbreviations()

                if $"{arrayItemItem.TypeDefinition.AccessPath}.{arrayItemItem.TypeDefinition.DisplayName}"
                   <> "Godot.Collections.Dictionary" then
                    false
                else
                    (returnType.GenericArguments |> Seq.head)
                        .GenericArguments
                        .Count = 0

    let generateInfo (contents: FSharpImplementationFileContents) =
        let entity, declarations =
            contents.Declarations
            |> List.choose
                (fun x ->
                    match x with
                    | FSharpImplementationFileDeclaration.Entity (entity, declarations) -> Some(entity, declarations)
                    | FSharpImplementationFileDeclaration.MemberOrFunctionOrValue _ -> None
                    | FSharpImplementationFileDeclaration.InitAction _ -> None)
            |> List.filter (fun (e, _) -> e.IsFSharpModule)
            |> List.head

        let entities =
            declarations
            |> List.choose
                (fun x ->
                    match x with
                    | FSharpImplementationFileDeclaration.Entity (entity, declarations) -> Some(entity, declarations)
                    | FSharpImplementationFileDeclaration.MemberOrFunctionOrValue _ -> None
                    | FSharpImplementationFileDeclaration.InitAction _ -> None)

        let state =
            entities
            |> List.choose (fun (entity, _) -> extractStateType entity)
            |> List.tryHead

        let node =
            entities
            |> List.choose (fun (entity, _) -> extractNodeType entity)
            |> List.tryHead


        match (node, state) with
        | None, None -> Result.Error [ "Base Type and State not found" ]
        | None, Some _ -> Result.Error [ "State not found" ]
        | Some _, None -> Result.Error [ "Base Type not found" ]
        | Some node, Some state ->
            let exportedFields = state.FSharpFields

            let notExportedFields =
                state.FSharpFields
                |> Seq.filter
                    (fun x ->
                        not
                        <| (x.PropertyAttributes
                            |> Seq.exists (fun x -> x.IsAttribute<ExportAttribute>())))

            let methods =
                declarations
                |> List.choose
                    (fun x ->
                        match x with
                        | FSharpImplementationFileDeclaration.Entity _ -> None
                        | FSharpImplementationFileDeclaration.MemberOrFunctionOrValue (value, _, _) -> Some(value)
                        | FSharpImplementationFileDeclaration.InitAction _ -> None)
                |> List.filter (fun x -> x.IsFunction && x.DeclaringEntity = Some(entity))
                |> List.ofSeq

            let errors =
                [ for method in methods do
                      let checkCustomMethod () =
                          if not
                             <| isValidNodeMethod method state node.TypeDefinition ExtraParamCountCheckMode.ZeroOrMore then
                              Some(
                                  $"{method.DisplayName} has an invalid signature. It should be '{node.TypeDefinition.DisplayName} [...] {state.DisplayName} -> {state.DisplayName}' or '{node.TypeDefinition.DisplayName} [...] {state.DisplayName} -> ({state.DisplayName}, <ReturnType>)'"
                              )
                          else
                              None

                      if method.DisplayName.StartsWith '_' then
                          if method.DisplayName = "_Ready" then
                              if not
                                 <| isValidReadySignature method state node.TypeDefinition then
                                  Some(
                                      $"_Ready should have the signature '{node.TypeDefinition.DisplayName} {state.DisplayName} -> {state.DisplayName}'"
                                  )
                              else
                                  None
                          elif method.DisplayName = "_Process" then
                              if not
                                 <| isValidProcessSignature method state node.TypeDefinition then
                                  Some(
                                      $"_Process should have the signature '{node.TypeDefinition.DisplayName} double {state.DisplayName} -> {state.DisplayName}'"
                                  )
                              else
                                  None
                          elif method.DisplayName = "_GetPropertyList" then
                              if not
                                 <| isValidGetPropertyListSignature method state node.TypeDefinition then
                                  Some(
                                      $"_GetPropertyList should have the signature '{node.TypeDefinition.DisplayName} {state.DisplayName} -> ({state.DisplayName}, Godot.Collections.Array<Godot.Collections.Dictionary>>"
                                  )
                              else
                                  None
                          else
                              checkCustomMethod ()
                      else
                          checkCustomMethod () ]
                |> List.choose id

            if errors |> List.length > 0 then
                Result.Error errors
            else
                let isOverride (method: FSharpMemberOrFunctionOrValue) =
                    let nodeMethods =
                        (GeneratorHelper.extractMethods node)
                        |> List.map (fun x -> x.DisplayName)

                    nodeMethods |> List.contains method.DisplayName

                let info =
                    {

                      Extending = new StringName(node.TypeDefinition.DisplayName)
                      ExtendingNamespace = GeneratorHelper.getScope node.TypeDefinition
                      Name = new StringName(entity.DisplayName)
                      methods =
                          [ for method in methods do
                                let isCurried = method.CurriedParameterGroups.Count >= 2

                                let returnParameter = method.ReturnParameter

                                let returnParameter =
                                    if returnParameter.Type.IsTupleType then
                                        let paramType =
                                            returnParameter.Type.GenericArguments |> Seq.tail |> Seq.head

                                        let typeName = GeneratorHelper.getTypeString paramType

                                        let paramType, propertyHint, hintString =
                                            match getTypeNameFromIdent.convertFSharpTypeToVariantType paramType with
                                            | None -> (Variant.Type.Nil, PropertyHint.None, "")
                                            | Some value ->
                                                match value with
                                                | None -> (Variant.Type.Nil, PropertyHint.None, "")
                                                | Some value -> value

                                        Some(
                                            { MethodParam.Name = "Return"
                                              OfTypeName = typeName
                                              OfType = paramType
                                              PropertyHint = propertyHint
                                              UsageFlags = PropertyUsageFlags.Default
                                              HintText = hintString }
                                        )
                                    else
                                        None

                                { MethodName = method.DisplayName
                                  IsOverride = isOverride method
                                  IsCurried = isCurried
                                  MethodParams =
                                      [ let getParamInfo (param: FSharpParameter) =
                                            let paramType = param.Type

                                            let typeName =
                                                GeneratorHelper.getTypeString (paramType.StripAbbreviations())

                                            let paramType, propertyHint, hintString =
                                                match getTypeNameFromIdent.convertFSharpTypeToVariantType paramType with
                                                | None -> (Variant.Type.Nil, PropertyHint.None, "")
                                                | Some value ->
                                                    match value with
                                                    | None -> (Variant.Type.Nil, PropertyHint.None, "")
                                                    | Some value -> value

                                            { MethodParam.Name = param.DisplayName
                                              OfTypeName = typeName
                                              OfType = paramType
                                              PropertyHint = propertyHint
                                              UsageFlags = PropertyUsageFlags.Default
                                              HintText = hintString }

                                        // The first and last parameters are internal parameters for fsharp
                                        if isCurried then
                                            for param in
                                                method.CurriedParameterGroups
                                                |> Seq.tail
                                                |> Seq.rev
                                                |> Seq.tail
                                                |> Seq.rev
                                                |> Seq.collect id do
                                                getParamInfo param
                                        else
                                            for param in
                                                method.CurriedParameterGroups
                                                |> Seq.head
                                                |> Seq.tail
                                                |> Seq.rev
                                                |> Seq.tail
                                                |> Seq.rev do
                                                getParamInfo param ]
                                  MethodFlags = MethodFlags.Default
                                  ReturnParameter = returnParameter } ]

                      StateToGenerate =
                          { Name = state.DisplayName
                            ExportedFields =
                                [ for field in exportedFields do
                                      let typeName =
                                          GeneratorHelper.getTypeString (field.FieldType.StripAbbreviations())

                                      let typ, propertyHint, hintString =
                                          match getTypeNameFromIdent.convertFSharpTypeToVariantType field.FieldType with
                                          | None -> (Variant.Type.Nil, PropertyHint.None, "")
                                          | Some value ->
                                              match value with
                                              | None -> (Variant.Type.Nil, PropertyHint.None, "")
                                              | Some value -> value

                                      { Name = field.DisplayName
                                        OfTypeName = typeName
                                        OfType = typ
                                        PropertyHint = propertyHint
                                        HintText = hintString
                                        UsageFlags = PropertyUsageFlags.Default } ]
                            InnerFields =
                                [ for field in notExportedFields do
                                      let typeName =
                                          GeneratorHelper.getTypeString (field.FieldType.StripAbbreviations())

                                      let typ, propertyHint, hintString =
                                          match getTypeNameFromIdent.convertFSharpTypeToVariantType field.FieldType with
                                          | None -> (Variant.Type.Nil, PropertyHint.None, "")
                                          | Some value ->
                                              match value with
                                              | None -> (Variant.Type.Nil, PropertyHint.None, "")
                                              | Some value -> value

                                      { Name = field.DisplayName
                                        OfTypeName = typeName
                                        OfType = typ
                                        PropertyHint = propertyHint
                                        HintText = hintString
                                        UsageFlags =
                                            PropertyUsageFlags.Default
                                            ||| PropertyUsageFlags.ScriptVariable } ] }
                      ModuleNameToOpen = $"{GeneratorHelper.getScope entity}.{entity.DisplayName}" }

                Result.Ok(info)
