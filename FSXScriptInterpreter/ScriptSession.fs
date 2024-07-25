﻿namespace FSXScriptCompiler

open System
open System.IO
open System.Text
open FSharp.Compiler.CodeAnalysis
open FSharp.Compiler.Interactive.Shell
open FSharp.Compiler.Symbols
open FSharp.Compiler.Text
open Godot.FSharp
open Godot.FSharp.ObjectGenerator
open Microsoft.FSharp.Core

type VariantType = GodotStubs.Type

type ScriptSession() as this =
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

    let mutable exportedFieldValues = Map.empty<string, obj>
    let internalFieldValues = Map.empty<string, obj>
    let mutable results: Option<FSharpParseFileResults> = None
    let mutable info : Option<ToGenerateInfo> = None
    let mutable checkResults : Option<FSharpCheckFileResults> = None


    let scriptInit =
        "#r \"Godot.Bindings\"
open Godot.NativeInterop

type InitFunc = delegate of nativeptr<GDExtensionInterface> -> Unit
let Init = Godot.Bridge.GodotBridge.Initialize

"

    do
        try
            // Directory.SetCurrentDirectory(AppContext.BaseDirectory)
            // fsiConfig.OnEvaluation
            // GD.Print("Test")

            fsiSession <- FsiEvaluationSession.Create(fsiConfig, allArgs, inStream, outStream, errStream)
        // let unmanagedCallbacks = nativeint(unmanagedCallbacks)

        // Godot.NativeInterop.NativeFuncs.Initialize(unmanagedCallbacks, unmanagedCallbacksSize)


        // fsiSession.AddBoundValue("unmanagedCallbacks", unmanagedCallbacks)
        // fsiSession.AddBoundValue("unmanagedCallbacksSize", unmanagedCallbacksSize)
        // fsiSession.EvalInteraction "printfn $\"unmanagedCallbacks: {unmanagedCallbacks}\" "
        // fsiSession.EvalInteraction "printfn $\"unmanagedCallbacksSize: {unmanagedCallbacksSize}\" "
        // let test = nativeint(int64)
        // fsiSession.EvalInteractionNonThrowing "#r \"GodotSharp.dll\"" |> ignore
        // fsiSession.EvalInteractionNonThrowing "open Godot" |> ignore
        // // fsiSession.EvalInteractionNonThrowing "Engine.IsEditorHint()" |> ignore
        // fsiSession.EvalInteractionNonThrowing "GD.Print(\"Hello\")" |> ignore
        with
        | (e: Exception) -> ()
    //     // AppContext.SetSwitch("Switch.System.Reflection.Assembly.SimulatedLocationInBaseDirectory", true)
    //     // let assembly = Assembly.LoadFrom "FSharp.Core.dll"
    //     // let _type = typeof<FsiEvaluationSession>
    //     let assembly = Assembly.LoadFrom($"{AppContext.BaseDirectory}/FSharp.Compiler.Service.dll")
    //     let _type = assembly.GetType(typeof<FsiEvaluationSession>.FullName)
    //     let create = _type.GetMethod("Create")
    //
    //     let test = create.GetParameters()
    //     for t in test do
    //         printf $"{t}"
    //
    //
    //     let params : obj array =
    //         [|fsiConfig, allArgs, inStream, outStream, errStream, Option<bool>.None, Option<LegacyReferenceResolver>.None|]
    //
    //
    //     let test = create.CreateDelegate(typeof<createSignature>)
    //     ()
    //     // let fsiSession = create.Invoke(_type, [|fsiConfig, allArgs, inStream, outStream, errStream, Option<bool>.None, Option<LegacyReferenceResolver>.None|]) :?> FsiEvaluationSession
    //     // // fsiSession <- handle.Unwrap() :?> FsiEvaluationSession
    //     // fsiSession <- FsiEvaluationSession.Create(fsiConfig, allArgs,
    //     //                                           inStream, outStream, errStream)
    //     // fsiSession.EvalExpression "printfn(\"Hello\")" |> ignore


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

    static member BasePath
        with get () = basePath
        and set (value) = basePath <- value

    member _.ClassName
        with get () =
            match info with
            | None -> ""
            | Some info ->                
                info.Name
                
    member _.BaseType
        with get () =
            match info with
            | None -> ""
            | Some info ->                
                info.Extending
    
    member _.PropertyList
        with get () =
            match info with
            | None -> List.empty
            | Some info -> info.StateToGenerate.ExportedFields

    member _.BuildDummy(name: string) =

        let scriptPath = Path.Join(basePath, $"{name}.fsx")
        let dll_path = Path.Join(basePath, $"{name}.dll")

        File.WriteAllText(
            scriptPath,
            $"""#r "Godot.Bindings"
open Godot.NativeInterop

type InitFunc = delegate of nativeptr<GDExtensionInterface> -> Unit
let Init = Godot.Bridge.GodotBridge.Initialize

module {name} =
    open Godot
    type PrintFunc = delegate of Unit -> Unit

    let Print() =
        GD.Print "999" """
        )

        checker.Compile(
            [| "fsc.exe"
               "-o"
               dll_path
               "-a"
               scriptPath
               "--targetprofile:netcore"
               "-r:addons/fsx-script/bin/Godot.Bindings.dll"
               "--debug" |]
        )
        |> Async.RunSynchronously
        |> ignore

        dll_path

    member _.eval(text: string) =
        try

            let init (unmanagedCallbacks: nativeint) (unmanagedCallbacksSize: int) = ()

            fsiSession.EvalInteraction "#r \"Godot.Bindings\""
            fsiSession.EvalInteraction "open Godot"
            fsiSession.EvalInteraction "open System.Runtime.InteropServices"

            let result =
                fsiSession.EvalExpression
                    "(Marshal.GetFunctionPointerForDelegate Godot.NativeInterop.NativeFuncs.Initialize).ToInt64"



            // let test  = (Marshal.GetFunctionPointerForDelegate Godot.NativeInterop.NativeFuncs.Initialize).ToInt64


            match result with
            | None -> ()
            | Some value -> ()


        // fsiSession.EvalInteraction "godot_print(System.String(\"From FSX\"))"

        // fsiSession.EvalInteraction "#r \"GodotSharp\""
        // fsiSession.EvalInteraction $"let unmanagedCallbacks = nativeint({unmanagedCallbacks}L)"
        // fsiSession.EvalInteraction $"let unmanagedCallbacksSize = {unmanagedCallbacksSize}"
        // fsiSession.EvalInteraction "Godot.NativeInterop.NativeFuncs.Initialize(unmanagedCallbacks, unmanagedCallbacksSize)"
        // fsiSession.EvalInteraction "open Godot"
        // fsiSession.EvalInteraction "printfn $\"{GD}\""
        // fsiSession.EvalInteraction "GD.Print(\"From FSX\")" |> ignore
        // fsiSession.EvalInteractionNonThrowing text |> ignore
        with
        | (e: Exception) -> ()

        if sbOut.Length > 0 then
            sbOut.Clear() |> ignore

        if sbErr.Length > 0 then
            sbErr.Clear() |> ignore



    member _.ParseScript(scriptCode: string) =
        let scriptPath = $"{this.ClassName}.fsx"

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
            | Some file -> Some(generateInfo file)

        checkResults <- answer

    member _.GetPropertyNames() =
        let exportedFields =
            match info with
            | None -> []
            | Some info ->
                info.StateToGenerate.ExportedFields
        exportedFields
        |> List.map _.Name     


    member _.Get(name: string, ret: outref<obj>) =
        match exportedFieldValues.TryFind name with
        | None ->
            ret <- null
            false
        | Some x ->
            ret <- x
            true

    member _.Set(name: string, value: obj) =
        match info
        with
        | None -> false
        | Some info ->  
            if info.StateToGenerate.ExportedFields |> List.exists (fun f -> f.Name = name) then
                exportedFieldValues <- exportedFieldValues |> Map.add name info
                true
            else
                false

// member _.CreateBaseObject():
