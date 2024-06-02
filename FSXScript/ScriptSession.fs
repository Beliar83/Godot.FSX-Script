namespace FSXScriptCompiler

open System
open System.Diagnostics
open System.IO
open System.Reflection
open System.Runtime.InteropServices.JavaScript
open System.Text
open FSharp.Compiler.CodeAnalysis
open FSharp.Compiler.Diagnostics
open FSharp.Compiler.Interactive.Shell
open FSharp.Compiler.Symbols
open FSharp.Compiler.Text
open Godot
open Microsoft.FSharp.Core

type ScriptSession() =
    let sbOut = StringBuilder()
    let sbErr = StringBuilder()
    let inStream = new StringReader("")
    let outStream = new StringWriter(sbOut)
    let errStream = new StringWriter(sbErr)

    let argv = [| "C:\\fsi.exe" |]

    let allArgs =
        Array.append argv [|
            "--noninteractive"
            // "--quiet"
            // "--gui-"
            // "--nologo"
            // "--noframework"
        |]

    let fsiConfig =
        FsiEvaluationSession.GetDefaultConfiguration()        
    
    let mutable fsiSession : FsiEvaluationSession = Unchecked.defaultof<_>

    let checker =
        FSharpChecker.Create(keepAssemblyContents = true)

    let mutable exportedFieldValues = Map.empty<string, Variant>
    let internalFieldValues = Map.empty<string, obj>
    let mutable allFields = List.empty<FSharpField>
    let mutable allMethods = List.empty<FSharpField>
    let mutable exportedFieldNames = List.empty<string>
    let mutable exportedMethodNames = List.empty<string>
    let mutable membersChanged = true
    let mutable assembly = Unchecked.defaultof<Assembly>

    let mutable results: Option<FSharpParseFileResults> = None
    let mutable answer: Option<FSharpCheckFileResults> = None


    do
        try
            // Directory.SetCurrentDirectory(AppContext.BaseDirectory)
            // fsiConfig.OnEvaluation
            // GD.Print("Test")
            
            fsiSession <- FsiEvaluationSession.Create(fsiConfig, allArgs,
            inStream, outStream, errStream)
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
        with (e : Exception) -> ()
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
        |> List.head


    let UpdateMembers () =
        let contents =
            match answer with
            | None -> None
            | Some answer ->
                match answer.ImplementationFile with
                | None -> None
                | Some value -> Some(value)

        let members =
            match contents with
            | None -> []
            | Some value ->
                let state =
                    value.Declarations
                    |> Seq.choose
                        (fun x ->
                            match x with
                            | FSharpImplementationFileDeclaration.Entity (entity, declarations) ->
                                Some(GetState declarations)
                            | FSharpImplementationFileDeclaration.MemberOrFunctionOrValue (value, curriedArgs, body) -> None
                            | FSharpImplementationFileDeclaration.InitAction action -> None)
                    |> Seq.tryHead

                match state with
                | None -> []
                | Some entity -> entity.FSharpFields |> List.ofSeq
            
        allFields <- members |> List.filter (fun m -> not <| m.FieldType.IsFunctionType)
        allMethods <- members |> List.filter (fun m -> m.FieldType.IsFunctionType)
        exportedFieldNames <- allFields |> List.map (fun x -> x.DisplayName)
        exportedMethodNames <- allMethods |> List.map (fun x -> x.DisplayName)
        membersChanged <- false 

    
    member this.BuildDummy(name: string) =
        let basePath = Directory.GetCurrentDirectory()
        
        let scriptPath = Path.Join(basePath, $"{name}.fsx")
        let dll_path = Path.Join(basePath, $"{name}.dll")
        File.WriteAllText(
                          scriptPath,
                          $"""#r "GodotSharp"
                          
open Godot.NativeInterop
open Godot

module {name} =
    type InitFunc = delegate of nativeint * int -> Unit
    type PrintFunc = delegate of Unit -> Unit

    let Init(unmanagedCallbacks : nativeint, unmanagedCallbacksSize : int) =
        Godot.NativeInterop.NativeFuncs.Initialize(unmanagedCallbacks, unmanagedCallbacksSize)
        GD.Print "FSX Init: Test"
    
    let Print() =
        GD.Print "999" """)
        checker.Compile([| "fsc.exe"; "-o"; dll_path; "-a"; scriptPath ; "--debug"|]) 
        |> Async.RunSynchronously |> ignore
        dll_path
    
    member this.eval(text: string) =
        let value = 
            try             
                
                let init (unmanagedCallbacks : nativeint) (unmanagedCallbacksSize : int) = ()
                    
                fsiSession.EvalInteraction "#r \"GodotSharpGDExtension\""
                fsiSession.EvalInteraction "open System.Runtime.InteropServices"
                    
                fsiSession.EvalExpression text
                
                // fsiSession.EvalInteraction "godot_print(System.String(\"From FSX\"))"
                    
                // fsiSession.EvalInteraction "#r \"GodotSharp\""            
                // fsiSession.EvalInteraction $"let unmanagedCallbacks = nativeint({unmanagedCallbacks}L)"
                // fsiSession.EvalInteraction $"let unmanagedCallbacksSize = {unmanagedCallbacksSize}"
                // fsiSession.EvalInteraction "Godot.NativeInterop.NativeFuncs.Initialize(unmanagedCallbacks, unmanagedCallbacksSize)"
                // fsiSession.EvalInteraction "open Godot"
                // fsiSession.EvalInteraction "printfn $\"{GD}\""
                // fsiSession.EvalInteraction "GD.Print(\"From FSX\")" |> ignore
                // fsiSession.EvalInteractionNonThrowing text |> ignore
            with (e : Exception) -> None
        if sbOut.Length > 0 then
            // GD.Print sbOut
            Console.Write sbOut
            sbOut.Clear() |> ignore            
        if sbErr.Length > 0 then
            Console.Error.Write sbErr
            // GD.Print sbErr
            sbErr.Clear() |> ignore
        value
        
    

    member this.ParseScriptFromPath(scriptPath: string) =
        this.ParseScriptFromCode(scriptPath, scriptPath |> File.ReadAllText)          
    member this.ParseScriptFromCode(scriptPath: string, scriptCode: string) =
        let scriptCode = SourceText.ofString scriptCode
        Environment.SetEnvironmentVariable("FSHARP_COMPILER_BIN", AppDomain.CurrentDomain.BaseDirectory)
        
        
        let options, diagnostics =
            checker.GetProjectOptionsFromScript(scriptPath, scriptCode)
            |> Async.RunSynchronously

        for diagnostic in diagnostics do
            printfn $"{diagnostic.Message}"

        // let options, diagnostics = checker.GetParsingOptionsFromProjectOptions options
        // for diagnostic in diagnostics do
        //     printfn $"{diagnostic.Message}"

        let _results, _answer =
            checker.ParseAndCheckFileInProject(scriptPath, 0, scriptCode, options)
            |> Async.RunSynchronously

        results <- Some(_results)

        answer <-
            match _answer with
            | FSharpCheckFileAnswer.Aborted -> None
            | FSharpCheckFileAnswer.Succeeded checkFileResults -> Some(checkFileResults)
        
        if answer.IsSome then
            printfn $"Compiling {scriptPath}"
            let assemblyPath = Path.ChangeExtension(scriptPath, ".dll")
            let errors, exitCode =
                checker.Compile([| "fsc.exe"; "-o"; assemblyPath; "-a"; scriptPath |])
                |> Async.RunSynchronously            
            for error in errors do printfn $"{error.Message} - {error.StartLine}:{error.StartColumn}-{error.EndLine}:{error.EndColumn}"
            if exitCode <> 0 || errors |> Array.exists (fun e -> e.Severity = FSharpDiagnosticSeverity.Error) then
                printfn "Compilation failed"
            else
                printfn "Compilation successful"
                assembly <- Assembly.Load(AssemblyName(Path.GetFileNameWithoutExtension(assemblyPath)))
                printfn "Defined"                
        else
            for diag in results.Value.Diagnostics do
                printfn $"{diag.Message}"



    member this.GetPropertyNames() =
        if membersChanged then
            UpdateMembers()
        exportedFieldNames
        |> List.map (fun x -> new StringName(x))
    // member this.GetMethods() =
    //     let infos =
    //         allMethods
    //         |> List.map (fun m ->
    //                 let info = new FSharpMethodInfo()
    //                 info.name <- new StringName(m.Name)
    //                 info
    //             )
    //     new MethodInfoVector(infos)
    //     
    // member this.GetPropertyList() =
    //     if fieldsChanged then
    //         UpdateFields()
    //     
    //     allFields
    //     |> List.map
    //         (fun x ->
    //             let paramType, propertyHint, hintString =
    //                 match getTypeNameFromIdent.convertFSharpTypeToVariantType x.FieldType with
    //                 | None -> (Variant.Type.NIL, PropertyHint.PROPERTY_HINT_NONE, "")
    //                 | Some value ->
    //                     match value with
    //                     | None -> (Variant.Type.NIL, PropertyHint.PROPERTY_HINT_NONE, "")
    //                     | Some value -> value
    //
    //             PropertyInfo(
    //                 paramType,
    //                 (StringName.op_Implicit x.DisplayName),
    //                 propertyHint,
    //                 hintString,
    //                 PropertyUsageFlags.Default,
    //                 true
    //             ))
    
    member this.Get (name : string, ret : outref<Variant>) =
        match exportedFieldValues.TryFind name with
        | None ->
            ret <- new Variant()
            false
        | Some x ->
            ret <- x
            ret <- x
            true
    
    member this.Set(name : string, value: Variant) =
        if exportedFieldNames |> List.contains name then
            exportedFieldValues <- exportedFieldValues |> Map.add name value
            true
        else
            false
            
        
    member this.Call(name : string, instance : GodotObject) =
        Console.WriteLine($"Session.Call {name}")
        
        if name = "test" then // exportedMethodNames |> List.contains name then
            try                
                let fsx = assembly.GetType("FSX")
                let method = fsx.GetMethod("test")
                method.Invoke(null, [|instance|]) |> ignore
            with (e : Exception) -> (printfn $"{e.Message}")
            if sbOut.Length > 0 then
                // GD.Print sbOut
                Console.Write sbOut
                sbOut.Clear() |> ignore            
            if sbErr.Length > 0 then
                Console.Write sbErr
                // GD.Print sbErr
                sbErr.Clear() |> ignore
        else
            Console.WriteLine($"{name} is not defined")
        ()
            
    // member this.CreateBaseObject():
