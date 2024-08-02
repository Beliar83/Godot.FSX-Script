namespace FSXScriptCompiler

open System
open System.IO
open System.Text
open FSharp.Compiler.CodeAnalysis
open FSharp.Compiler.Interactive.Shell
open FSharp.Compiler.Symbols
open FSharp.Compiler.Text
open FSharpx.Collections
open Godot
open Godot.Bridge
open Godot.Collections
open Godot.FSharp.ObjectGenerator
open Microsoft.FSharp.Core

type VariantType = Godot.VariantType

type ScriptSession() as this =
    inherit Resource()   
        
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

    member _.GetClassName() =
        match info with
            | None -> new StringName("")
            | Some info ->                
                info.Name
                
    member _.GetBaseType() =
        match info with
            | None -> new StringName("")
            | Some info ->                
                info.Extending
    
    member _.PropertyList
        with get () =
            match info with
            | None -> List.empty
            | Some info -> info.StateToGenerate.ExportedFields
        
    member val PropertyTypes = PersistentHashMap.empty<StringName, Godot.VariantType> with get, set
    
    static member BindMethods(context : ClassDBRegistrationContext) =
        context.BindConstructor(fun () -> new ScriptSession())
        
        context.BindStaticMethod(new StringName("SetBasePath"), new ParameterInfo(new StringName("basePath"), VariantType.String),  fun (value : string) -> basePath <- value);
        
        let mutable returnInfo = new ReturnInfo(VariantType.StringName)
        context.BindMethod(new StringName(nameof(Unchecked.defaultof<ScriptSession>.GetClassName)), returnInfo, fun (session : ScriptSession) -> session.GetClassName())        
        context.BindMethod(new StringName(nameof(Unchecked.defaultof<ScriptSession>.GetBaseType)), returnInfo, fun (session : ScriptSession) -> session.GetBaseType())        
        context.BindMethod(new StringName(nameof(Unchecked.defaultof<ScriptSession>.ParseScript)), new ParameterInfo(new StringName("code"), VariantType.String), fun (session : ScriptSession) (code : string) -> session.ParseScript(code))        
        context.BindMethod(new StringName(nameof(Unchecked.defaultof<ScriptSession>.GetPropertyList)), new ParameterInfo(new StringName("propertyList"), VariantType.Array, Hint = PropertyHint.ArrayType, HintString = "Dictionary"), fun (session : ScriptSession) (propertyList: GodotArray<GodotDictionary>) -> session.GetPropertyList(propertyList))              
        let mutable returnInfo = new ReturnInfo(VariantType.Bool)
        context.BindMethod(new StringName(nameof(Unchecked.defaultof<ScriptSession>.HasProperty)), new ParameterInfo(new StringName("name"), VariantType.String), returnInfo, fun (session : ScriptSession) (name: StringName) -> session.HasProperty(name))
        

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
        let scriptPath = $"{this.GetClassName()}.fsx"

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
        
        this.PropertyTypes <-
            this.PropertyList
            |> List.map (fun p -> (p.Name, p.OfType))
            |> PersistentHashMap.ofSeq 
        

    member _.GetPropertyList(propertyList : GodotArray<GodotDictionary>) =
        for field in this.PropertyList do
            let propertyInfo = new GodotDictionary()
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
    
    member _.HasProperty(name: StringName) =
        match info
        with
        | None -> false
        | Some info ->
            info.StateToGenerate.ExportedFields |> List.exists (fun f -> f.Name = name)    

    member _.GetPropertyNames() =
        let exportedFields =
            match info with
            | None -> []
            | Some info ->
                info.StateToGenerate.ExportedFields
        exportedFields
        |> List.map _.Name     

