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
open Godot.FSharp.Variant
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
    
    static let getFrameworkReferences =
        let runtimePath = RuntimeEnvironment.GetRuntimeDirectory()
        let dotnetVersion = Environment.Version        
        
        let basePath = Path.GetFullPath $"{runtimePath}/../../../packs/Microsoft.NETCore.App.Ref/{dotnetVersion.Major}.{dotnetVersion.Minor}.{dotnetVersion.Build}/ref/net{dotnetVersion.Major}.{dotnetVersion.Minor}"
        Directory.GetFiles(basePath, "*.dll")
        
    static let getGodotReferences =
        let executableRoot = Path.GetDirectoryName(OS.GetExecutablePath())
        [|Path.Join(executableRoot, "GodotSharp/Api/Release", "GodotSharp.dll")|]
    
    static let otherFlags =
        [|"--noframework"|]
        |> Array.append (getGodotReferences |> Array.map (fun x -> $"-r:{x}"))
        |> Array.append (getFrameworkReferences |> Array.map (fun x -> $"-r:{x}"))
    
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

    static member Validate(
            script: string,
            path: string,
            validateFunctions: bool,
            validateErrors: bool,
            validateWarnings: bool,
            validateSafeLines: bool
        ) =
        
        let checker = FSharpChecker.Create(keepAssemblyContents = true)
        
        let validationResult = new Dictionary()

        let script_path =
            if String.IsNullOrWhiteSpace path then "dummy.fsx"
            elif not <| path.EndsWith ".fsx" then $"{path}.fsx"
            else path

        let script_path =
            let index = script_path.IndexOf("://")
            if index >= 0 then script_path.Substring(index + 3) else script_path

        let scriptCode = script |> SourceText.ofString       

        Environment.SetEnvironmentVariable("FSHARP_COMPILER_BIN", AppDomain.CurrentDomain.BaseDirectory)

        let options, _diagnostics =
            checker.GetProjectOptionsFromScript(script_path, scriptCode, otherFlags = otherFlags)
            |> Async.RunSynchronously

        let parseResults, answer =
            checker.ParseAndCheckFileInProject(script_path, 0, scriptCode, options)
            |> Async.RunSynchronously
        
        let isValid, checkFileResults =
            match answer with
            | FSharpCheckFileAnswer.Aborted -> (false, None)
            | FSharpCheckFileAnswer.Succeeded checkFileResults ->  (true, Some(checkFileResults))
        

        let results =
            match parseResults with
            | fileResults -> fileResults

        
        
        let diagnostics =
            results.Diagnostics
            |> Array.append
            <| match checkFileResults with
                | None -> [||]
                | Some value -> value.Diagnostics
        
        if diagnostics.Length > 0 then
            let isValid =
                not
                <| (diagnostics
                    |> Array.exists (fun x -> x.Severity = FSharpDiagnosticSeverity.Error))

            validationResult.Add("valid", isValid)

            if validateErrors then
                let errorList = new Array()

                for error in
                        diagnostics
                    |> Array.filter (fun x -> x.Severity = FSharpDiagnosticSeverity.Error) do
                    let errorData = new Dictionary()
                    errorData.Add("line", error.StartLine)
                    errorData.Add("column", Variant.CreateFrom(error.StartColumn + 1))
                    errorData.Add("message", error.Message)
                    errorData.Add("path", path)
                    
                    errorList.Add(errorData)

                validationResult["errors"] <- errorList

            if validateWarnings then
                let warningList = new Array()

                for warning in
                        diagnostics
                    |> Array.filter (fun x -> x.Severity = FSharpDiagnosticSeverity.Warning) do
                    let warningData = new Dictionary()
                    // TODO: Make example project and create bug
                    warningData.Add("start_line", warning.StartLine)
                    warningData.Add("end_line", warning.EndLine)
                    warningData.Add("leftmost_column", warning.StartColumn)
                    warningData.Add("rightmost_column", warning.EndColumn)
                    warningData.Add("code", warning.ErrorNumber)
                    warningData.Add("string_code", warning.ErrorNumberText)
                    warningData.Add("message", warning.Message)
                    warningList.Add(warningData)

                validationResult["warnings"] <- warningList
        else
            validationResult.Add("valid", isValid)

        if validateFunctions then
            let functions =
                match checkFileResults with
                | None -> []
                | Some value ->
                    match value.ImplementationFile with
                    | None -> []
                    | Some value ->
                        let entity, declarations =
                            value.Declarations
                            |> List.choose (fun x ->
                                match x with
                                | FSharpImplementationFileDeclaration.Entity(entity, declarations) -> Some(entity, declarations)
                                | FSharpImplementationFileDeclaration.MemberOrFunctionOrValue _ -> None
                                | FSharpImplementationFileDeclaration.InitAction _ -> None)
                            |> List.filter (fun (e, _) -> e.IsFSharpModule)
                            |> List.head                        
                        declarations
                        |> List.choose (fun x ->
                            match x with
                            | FSharpImplementationFileDeclaration.Entity _ -> None
                            | FSharpImplementationFileDeclaration.MemberOrFunctionOrValue(value, _, _) ->
                                Some(value)
                            | FSharpImplementationFileDeclaration.InitAction _ -> None)
                        |> List.filter (fun  x -> x.IsFunction && x.DeclaringEntity = Some(entity))
                        |> List.map (fun x -> $"{x.DisplayName}:{x.DeclarationLocation.StartLine}")              
                |> Seq.ofList
                
            validationResult.Add("functions", Array<string>(functions))
        
        if validateSafeLines then
            validationResult.Add("safe_lines", Array<int>())
                
        validationResult
    
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

    member _.Compile(scriptCode: string, scriptPath: string) =
        match (info, results, checkResults) with
        | Some info, Some results, Some checkResults ->
            let diagnostics =
                results.Diagnostics
                |> Array.append
                <| checkResults.Diagnostics
            
            if not <| (diagnostics |> Array.exists (fun diagnostic -> diagnostic.Severity = FSharpDiagnosticSeverity.Error)) then
                // TODO: Write C# classes for export
                
                let fsharpCompileFile = FileAccess.CreateTemp(int <| FileAccess.Write, Path.GetFileNameWithoutExtension scriptPath, ".fsx")
                
                let propertyNames =
                    info.StateToGenerate.ExportedFields
                    |> List.map _.Name                   
                    
                let addPropertyNamesModule(builder : StringBuilder) =
                    propertyNames
                    |> List.fold (fun (builder : StringBuilder) name -> builder.AppendLine($"\tlet {name} = new Godot.StringName(\"{name}\")"))
                           (builder.AppendLine("module __PropertyNames ="))                    
                
                let isSinglePrecision field = field.OfTypeName.ToString() = "System.Single"
                let addDefaultStateFunction(builder : StringBuilder) =
                    let builder = builder.AppendLine("let __get_default_state() =")
                    let getDefaultForPrecision field = if isSinglePrecision field then "0f" else "0"
                    let getDefaultForField field =
                        match getGodotDefaultForGodotSharp(field.OfType) with
                        | DefaultValue.Simple value -> $"{field.Name} = {value}"
                        | DefaultValue.Nil -> $"{field.Name} = null"
                        | DefaultValue.Int -> $"{field.Name} = 0"
                        | DefaultValue.Float -> $"{field.Name} = { getDefaultForPrecision(field) }"
                        | DefaultValue.Object -> $"{field.Name} = new {field.OfTypeName}()"
                    
                    info.StateToGenerate.ExportedFields
                    |> List.map getDefaultForField
                    |> List.fold (fun (builder : StringBuilder) value -> builder.AppendLine($"\t\t{value}")) (builder.AppendLine("\t{"))
                    |> _.AppendLine("\t}")
                
                let getConversionForPrecision field = if isSinglePrecision field then "Single" else "Double"
                let systemTypePrefixLength = "System.".Length
                    
                let getConversionForField valueName field =
                    match getConversionToDotnetForGodotSharp(field.OfType) with
                    | Simple methodName -> $"value.{methodName}()"
                    | Nil -> "null"
                    | Int -> $"{valueName}.As{field.OfTypeName.ToString().Remove(0, systemTypePrefixLength)}()"
                    | Float -> $"{valueName}.As{getConversionForPrecision field}()"
                    | Object -> $"{valueName}.AsGodotObject() :?> {field.OfTypeName}"
                let addSetMethod(builder: StringBuilder) =                        
                    let unknownPropertyMessage = $"$\"__set: {info.Name} has no exported value '{{name}}'\""
                    let getConversionForField = getConversionForField "value"
                    info.StateToGenerate.ExportedFields
                    |> List.fold ( fun (builder : StringBuilder) field ->
                                    builder
                                        .AppendLine($"if name = __PropertyNames.{field.Name} then")
                                        .AppendLine($"\t\t{{ state with {field.Name} = {getConversionForField(field)} }}")
                                        .Append("\telse ")
                            ) (builder.AppendLine("let __set(state: State, name: Godot.StringName, value: Godot.Variant) =").Append("\t"))
                    |> _.AppendLine()
                    |> _.AppendLine($"\t\tGodot.GD.PrintErr {unknownPropertyMessage}")
                    |> _.AppendLine("\t\tstate")
                
                let addGetMethod(builder: StringBuilder) =
                    let unknownPropertyMessage = $"$\"__get: {info.Name} has no exported value '{{name}}'\""
                    propertyNames
                    |> List.fold (fun (builder : StringBuilder) name ->
                            builder
                                .AppendLine($"if name = __PropertyNames.{name} then")
                                .AppendLine($"\t\tGodot.Variant.CreateFrom state.{name}")
                                .Append("\telse ")
                            ) (builder.AppendLine("let __get(state : State, name: Godot.StringName) =").Append("\t"))
                    |> _.AppendLine()
                    |> _.AppendLine($"\t\tGodot.GD.PrintErr {unknownPropertyMessage}")
                    |> _.AppendLine("\t\tnew Godot.Variant()")
                
                let addCallMethod(builder: StringBuilder) =
                    let unknownMethodMessage = $"$\"__call: {info.Name} has no method '{{methodName}}'\""
                    info.methods
                    |> List.fold (fun (builder : StringBuilder) method ->
                            let methodParams =
                                method.MethodParams
                                |> List.indexed
                                |> List.map (fun (index, parameter) ->
                                        let valueName = $"arguments[{index}]"
                                        $"{getConversionForField valueName parameter}"
                                    )


                            let methodParams =
                                if methodParams.Length > 0 then
                                    if method.IsCurried then
                                        methodParams
                                        |> String.concat " "
                                    else                                    
                                        methodParams
                                        |> String.concat ","
                                        |> sprintf ", %s"
                                else
                                    ""
                                    
                            let stateProcessing, returnValue =
                                match method.ReturnParameter with
                                | None -> ("callResult", "new Godot.Variant()")
                                | Some _ -> ("fst callResult", "Variant.CreateFrom(snd callResult)")
                            
                            let call =
                                if method.IsCurried then
                                    $" self{methodParams} state"
                                else
                                    $"(self{methodParams}, state)"
                            builder
                                .AppendLine($"if methodName = new StringName(\"{method.MethodName}\") && arguments.Count = {method.MethodParams.Length} then")
                                .AppendLine($"\t\tlet callResult = {method.MethodName}{call}")
                                .AppendLine($"\t\tstate <- {stateProcessing}")
                                .AppendLine($"\t\t{returnValue}")
                                .Append("\telse ")
                            ) ( builder.AppendLine("let __call(self: Base, methodName: StringName,  state: byref<State>, arguments: Godot.Collections.Array<Godot.Variant>) =").Append("\t"))
                    |> _.AppendLine()
                    |> _.AppendLine($"\t\tGodot.GD.PrintErr {unknownMethodMessage}")
                    |> _.AppendLine("\t\tnew Godot.Variant()")
                
                
                
                
                let addStoreStateMethod(builder: StringBuilder) =
                    propertyNames
                    |> List.fold (fun (builder: StringBuilder) name ->
                            let valueName = $"stateDict[\"{name}\"]"
                            builder
                                .AppendLine($"\t{valueName} <- state.{name}")
                        ) (builder.AppendLine("let __storeState(state : State) =").AppendLine("\tlet mutable stateDict = new Godot.Collections.Dictionary()"))
                    |> _.AppendLine("\tstateDict")
                
                let addRestoreStateMethod(builder: StringBuilder) =
                    info.StateToGenerate.ExportedFields
                    |> List.fold ( fun (builder : StringBuilder) field ->
                                    let valueName = $"stateDict[\"{field.Name}\"]"
                                    builder
                                        .AppendLine($"\t\t{field.Name} = {getConversionForField valueName field}")
                            ) (builder.AppendLine("let __restoreState(stateDict: Godot.Collections.Dictionary) =").AppendLine("\t{"))
                    |> _.AppendLine("\t}")               
                
                let compileCodeBuilder =
                    StringBuilder()
                        |> _.Append(scriptCode)
                        |> _.AppendLine()
                        |> _.AppendLine("#nowarn \"0067\"")
                        |> _.AppendLine()
                        |> addPropertyNamesModule
                        |> _.AppendLine()
                        |> addDefaultStateFunction
                        |> _.AppendLine()
                        |> addSetMethod
                        |> _.AppendLine()
                        |> addGetMethod
                        |> _.AppendLine()
                        |> addCallMethod
                        |> _.AppendLine()
                        |> addStoreStateMethod
                        |> _.AppendLine()
                        |> addRestoreStateMethod
                        |> _.AppendLine()
                
                let code = compileCodeBuilder.ToString().Replace("\t", "    ")
                
                if not <| fsharpCompileFile.StoreString code then
                    GD.PrintErr "Could not write temporary script file for compilation"
                else               
                    fsharpCompileFile.Close()
                    let tempScriptPath = Path.GetFullPath(fsharpCompileFile.GetPath())
                    let args =
                        otherFlags
                        |> Array.append [| "fsc.exe"; "-a"; $"\"{tempScriptPath}\""; "-o" ; Path.ChangeExtension(scriptPath, "dll") |]
                        
                    let task =
                        checker.Compile(args)
                        |> Async.StartAsTask
                        
                    task.Wait()
                    
                    File.Delete(tempScriptPath)
        | _ -> ()

                
    
        
    member _.ParseScript(scriptCode: string, scriptPath: string) =

        Environment.SetEnvironmentVariable("FSHARP_COMPILER_BIN", AppDomain.CurrentDomain.BaseDirectory)

        let parseResults, parseAnswer =
            let scriptCode = scriptCode |> SourceText.ofString
            let options, _ =
                checker.GetProjectOptionsFromScript(scriptPath, scriptCode, otherFlags = otherFlags)
                |> Async.RunSynchronously
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
                | Error messages ->
                    for message in messages do
                        GD.PrintErr $"Error parsing {scriptPath}: {message}"
                    None
                | Ok info ->                    
                    Some(info)

        checkResults <- answer

        this.PropertyTypes <-
            this.PropertyList
            |> List.map (fun p -> (new StringName(p.Name), p.OfType))
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
        | Some info -> info.StateToGenerate.ExportedFields |> List.exists (fun f -> f.Name = name.ToString())

    member _.GetPropertyNames() =
        let exportedFields =
            match info with
            | None -> []
            | Some info -> info.StateToGenerate.ExportedFields

        exportedFields |> List.map _.Name

    member _.CanInstantiate() =
        match info with
        | None -> false
        | Some _ -> true
