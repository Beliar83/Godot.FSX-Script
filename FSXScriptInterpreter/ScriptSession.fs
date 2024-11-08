namespace FSXScriptCompiler

open System
open System.IO
open System.Text
open FSharp.Compiler.CodeAnalysis
open FSharp.Compiler.Interactive.Shell
open FSharp.Compiler.Symbols
open FSharp.Compiler.Text
open Godot.FSharp
open Microsoft.FSharp.Core

type VariantType = GodotStubs.Type

type ScriptSession() =
    static let mutable basePath: string = ""
    let sbOut = StringBuilder()
    let sbErr = StringBuilder()
    let inStream = new StringReader("")
    let outStream = new StringWriter(sbOut)
    let errStream = new StringWriter(sbErr)

    let argv = [| "C:\\fsi.exe" |]

    let allArgs =
        Array.append
            argv
            [| "--noninteractive"
            // "--quiet"
            // "--gui-"
            // "--nologo"
            // "--noframework"
             |]

    let fsiConfig =
        FsiEvaluationSession.GetDefaultConfiguration()

    let mutable fsiSession: FsiEvaluationSession = Unchecked.defaultof<_>

    let checker =
        FSharpChecker.Create(keepAssemblyContents = true)

    let mutable className: string = "dummy"

    let mutable exportedFieldValues = Map.empty<string, obj>
    let internalFieldValues = Map.empty<string, obj>
    let mutable allFields = List.empty<FSharpField>
    let mutable exportedFieldNames = List.empty<string>
    let mutable fieldsChanged = true

    let mutable results: Option<FSharpParseFileResults> = None
    let mutable answer: Option<FSharpCheckFileResults> = None


    let scriptInit =
        "#r \"Godot.Bindings\"
open Godot.NativeInterop

type InitFunc = delegate of nativeptr<GDExtensionInterface> -> Unit
let Init = Godot.Bridge.GodotBridge.Initialize

"

    do
        try
            fsiSession <- FsiEvaluationSession.Create(fsiConfig, allArgs, inStream, outStream, errStream)
        with
        | (e: Exception) -> ()

    let GetExportedPropertiesOfEntity (declarations: FSharpImplementationFileDeclaration list) =
        let fields =
            declarations
            |> List.choose
                (fun x ->
                    match x with
                    | FSharpImplementationFileDeclaration.Entity (entity, declarations) -> None
                    | FSharpImplementationFileDeclaration.MemberOrFunctionOrValue (value, curriedArgs, body) ->
                        if value.IsValue then
                            Some(value)
                        else
                            None
                    | FSharpImplementationFileDeclaration.InitAction action -> None)

        [ for field in fields do
              field ]

    let GetState (declarations: FSharpImplementationFileDeclaration list) =
        declarations
        |> List.choose
            (fun x ->
                match x with
                | FSharpImplementationFileDeclaration.Entity (entity, declarations) -> Some((entity))
                | FSharpImplementationFileDeclaration.MemberOrFunctionOrValue (value, curriedArgs, body) -> None
                | FSharpImplementationFileDeclaration.InitAction action -> None)
        |> List.filter (fun (entity) -> entity.DisplayName = "State")
        |> List.tryHead


    let UpdateFields () =
        let contents =
            match answer with
            | None -> None
            | Some answer ->
                match answer.ImplementationFile with
                | None -> None
                | Some value -> Some(value)

        let fields =
            match contents with
            | None -> []
            | Some value ->
                let state =
                    value.Declarations
                    |> Seq.choose
                        (fun x ->
                            match x with
                            | FSharpImplementationFileDeclaration.Entity (entity, declarations) -> GetState declarations
                            | FSharpImplementationFileDeclaration.MemberOrFunctionOrValue (value, curriedArgs, body) ->
                                None
                            | FSharpImplementationFileDeclaration.InitAction action -> None)
                    |> Seq.tryHead

                match state with
                | None -> []
                | Some entity -> entity.FSharpFields |> List.ofSeq

        allFields <- fields
        exportedFieldNames <- fields |> List.map (fun x -> x.DisplayName)
        fieldsChanged <- false

    static member BasePath
        with get () = basePath
        and set (value) = basePath <- value

    member _.ClassName
        with get () = className
        and set (value) = className <- value

    member _.ParseScript(scriptCode: string) =
        let scriptPath = "script.fsx"

        let scriptCode =
            $"{scriptCode}{scriptInit}" |> SourceText.ofString

        Environment.SetEnvironmentVariable("FSHARP_COMPILER_BIN", AppDomain.CurrentDomain.BaseDirectory)

        let options, diagnostics =
            checker.GetProjectOptionsFromScript(scriptPath, scriptCode)
            |> Async.RunSynchronously

        let parseResults, parseAnswer =
            checker.ParseAndCheckFileInProject(scriptPath, 0, scriptCode, options)
            |> Async.RunSynchronously

        results <- Some(parseResults)

        answer <-
            match parseAnswer with
            | FSharpCheckFileAnswer.Aborted -> None
            | FSharpCheckFileAnswer.Succeeded checkFileResults -> Some(checkFileResults)

    member _.GetPropertyNames() =
        if fieldsChanged then UpdateFields()
        exportedFieldNames

    member _.Get(name: string, ret: outref<obj>) =
        match exportedFieldValues.TryFind name with
        | None ->
            ret <- null
            false
        | Some x ->
            ret <- x
            true

    member _.Set(name: string, value: obj) =
        if exportedFieldNames |> List.contains name then
            exportedFieldValues <- exportedFieldValues |> Map.add name value
            true
        else
            false
