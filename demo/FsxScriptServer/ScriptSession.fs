namespace Godot.FSharp

open System
open System.IO
open System.Runtime.InteropServices
open System.Text
open FSharp.Compiler.CodeAnalysis
open FSharp.Compiler.Diagnostics
open FSharp.Compiler.Interactive.Shell
open FSharp.Compiler.Symbols
open FSharp.Compiler.Text
open FSharpx.Collections
open Godot
open Godot.Collections
open Godot.FSharp.ObjectGenerator
open Microsoft.FSharp.Core

type VariantType = Variant.Type

type ScriptSession() as this =
    static let mutable basePath: string = ""
    static let propertyInfoName = new StringName("Name")
    static let propertyInfoClassName = new StringName("ClassName")
    static let propertyInfoType = new StringName("Type")
    static let propertyInfoHint = new StringName("Hint")
    static let propertyInfoHintString = new StringName("HintString")
    static let propertyInfoUsage = new StringName("Usage")

    let sbOut = StringBuilder()
    let sbErr = StringBuilder()
    let inStream = new StringReader("")
    let outStream = new StringWriter(sbOut)
    let errStream = new StringWriter(sbErr)

    let argv = [| "C:\\fsi.exe" |]

    let allArgs = Array.append argv [| "--noninteractive" |]

    let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()

    let mutable fsiSession: FsiEvaluationSession = Unchecked.defaultof<_>

    let checker = FSharpChecker.Create(keepAssemblyContents = true)

    let mutable results: Option<FSharpParseFileResults> = None
    let mutable info: Option<ToGenerateInfo> = None
    let mutable checkResults: Option<FSharpCheckFileResults> = None

    static let scriptInit =
        let versionInfo = Engine.GetVersionInfo()
        let major = versionInfo["major"]
        let minor = versionInfo["minor"]
        let patch = versionInfo["patch"]
        let status = versionInfo["status"]

        let status =
            if status = Variant.CreateFrom "stable" then
                ""
            else
                $".{status}"
        //"\n#r \"nuget: Godot.Bindings, {}.{}.*-*\"", version_info.get("major").unwrap(), version_info.get("minor").unwrap()
        $"#r \"GodotSharp\", {major}.{minor}.{patch}{status}"

    do
        try
            fsiSession <- FsiEvaluationSession.Create(fsiConfig, allArgs, inStream, outStream, errStream)
        with (e: Exception) ->
            ()



    let GetExportedPropertiesOfEntity (declarations: FSharpImplementationFileDeclaration list) =
        let fields =
            declarations
            |> List.choose (fun x ->
                match x with
                | FSharpImplementationFileDeclaration.Entity(_entity, _declarations) -> None
                | FSharpImplementationFileDeclaration.MemberOrFunctionOrValue(value, _curriedArgs, _body) ->
                    if value.IsValue then Some(value) else None
                | FSharpImplementationFileDeclaration.InitAction _action -> None)

        [ for field in fields do
              field ]

    let GetState (declarations: FSharpImplementationFileDeclaration list) =
        declarations
        |> List.choose (fun x ->
            match x with
            | FSharpImplementationFileDeclaration.Entity(entity, _declarations) -> Some entity
            | FSharpImplementationFileDeclaration.MemberOrFunctionOrValue(_value, _curriedArgs, _body) -> None
            | FSharpImplementationFileDeclaration.InitAction _action -> None)
        |> List.filter (fun entity -> entity.DisplayName = "State")
        |> List.tryHead

    static member SetBasePath(path: String) = basePath <- path

    member _.GetClassName() =
        match info with
        | None -> new StringName("")
        | Some info -> info.Name

    member _.GetBaseType() =
        match info with
        | None -> new StringName("")
        | Some info -> info.Extending

    member _.PropertyList =
        match info with
        | None -> List.empty
        | Some info -> info.StateToGenerate.ExportedFields

    member val PropertyTypes = PersistentHashMap.empty<StringName, Variant.Type> with get, set

    member _.ParseScript(scriptCode: string, scriptPath: string) =
        let scriptCode = $"{scriptCode}{scriptInit}" |> SourceText.ofString

        Environment.SetEnvironmentVariable("FSHARP_COMPILER_BIN", AppDomain.CurrentDomain.BaseDirectory)

        let options, _diagnostics =
            checker.GetProjectOptionsFromScript(scriptPath, scriptCode)
            |> Async.RunSynchronously

        let parseResults, parseAnswer =
            checker.ParseAndCheckFileInProject(scriptPath, 0, scriptCode, options)
            |> Async.RunSynchronously

        results <- Some(parseResults)

        let answer =
            match parseAnswer with
            | FSharpCheckFileAnswer.Aborted -> None
            | FSharpCheckFileAnswer.Succeeded checkFileResults -> Some(checkFileResults)

        let file =
            match answer with
            | None -> None
            | Some value -> value.ImplementationFile

        info <-
            match file with
            | None -> None
            | Some file ->
                match generateInfo file with
                | Error message ->
                    GD.PrintErr $"Error parsing {scriptPath}: {message}"
                    None
                | Ok info -> Some(info)


        checkResults <- answer

        this.PropertyTypes <-
            this.PropertyList
            |> List.map (fun p -> (p.Name, p.OfType))
            |> PersistentHashMap.ofSeq


    member _.GetProperties() =
        let propertyList = Array<Dictionary>()

        for field in this.PropertyList do
            let propertyInfo = new Dictionary()
            propertyInfo.Add(propertyInfoName, field.Name)

            if field.OfType = VariantType.Object then
                propertyInfo.Add(propertyInfoClassName, field.OfTypeName)
            else
                propertyInfo.Add(propertyInfoClassName, new StringName(""))

            propertyInfo.Add(propertyInfoType, Variant.From(&field.OfType))
            propertyInfo.Add(propertyInfoHint, Variant.From(&field.PropertyHint))
            propertyInfo.Add(propertyInfoHintString, Variant.From(&field.HintText))
            propertyInfo.Add(propertyInfoUsage, Variant.From(&field.UsageFlags))
            propertyList.Add(propertyInfo)

        propertyList

    member _.HasProperty(name: StringName) =
        match info with
        | None -> false
        | Some info -> info.StateToGenerate.ExportedFields |> List.exists (fun f -> f.Name = name)

    member _.GetPropertyNames() =
        let exportedFields =
            match info with
            | None -> []
            | Some info -> info.StateToGenerate.ExportedFields

        exportedFields |> List.map _.Name

    member _.Validate
        (
            script: string,
            path: string,
            validateFunctions: bool,
            validateErrors: bool,
            validateWarnings: bool,
            _validateSafeLines: bool
        ) =
        // TODO: Check
        let validationResult = new Dictionary()

        let path =
            if String.IsNullOrWhiteSpace path then "dummy.fsx"
            elif not <| path.EndsWith ".fsx" then $"{path}.fsx"
            else path

        let path =
            let index = path.IndexOf("://")
            if index >= 0 then path.Substring(index + 3) else path

        match results with
        | None -> validationResult.Add("valid", false)
        | Some parseResults ->
            let results =
                match parseResults with
                | fileResults -> fileResults

            if results.Diagnostics.Length > 0 then
                let isValid =
                    not
                    <| (results.Diagnostics
                        |> Array.exists (fun x -> x.Severity = FSharpDiagnosticSeverity.Error))

                printfn $"Valid: {isValid}"
                validationResult.Add("valid", isValid)

                if validateErrors then
                    printfn "Getting errors"
                    let errorList = new Array()
                    // TODO: Make example project and create bug
                    GCHandle.Alloc(errorList) |> ignore

                    for error in
                        results.Diagnostics
                        |> Array.filter (fun x -> x.Severity = FSharpDiagnosticSeverity.Error) do
                        let errorData = new Dictionary()
                        GCHandle.Alloc(errorData) |> ignore
                        errorData.Add("line", error.StartLine)
                        errorData.Add("column", error.StartColumn)
                        errorData.Add("message", error.Message)
                        errorList.Add(errorData)

                    validationResult["errors"] <- errorList

                if validateWarnings then
                    printfn "Getting Warnings"
                    let warningList = new Array()
                    GCHandle.Alloc(warningList) |> ignore

                    for warning in
                        results.Diagnostics
                        |> Array.filter (fun x -> x.Severity = FSharpDiagnosticSeverity.Warning) do
                        let warningData = new Dictionary()
                        // TODO: Make example project and create bug
                        GCHandle.Alloc(warningData) |> ignore
                        warningData.Add("start_line", warning.StartLine)
                        warningData.Add("end_line", warning.EndLine)
                        warningData.Add("leftmost_column", warning.StartColumn)
                        warningData.Add("rightmost_column", warning.EndColumn)
                        warningData.Add("code", warning.ErrorNumber)
                        warningData.Add("string_code", warning.ErrorNumberText)
                        warningData.Add("message", warning.Message)
                        warningList.Add(warningData)

                    validationResult["warnings"] <- warningList

                printfn "ScriptSession: Returning"
            else
                printfn "Valid: true"
                validationResult.Add("valid", true)
                printfn "ScriptSession: Returning"

        validationResult
