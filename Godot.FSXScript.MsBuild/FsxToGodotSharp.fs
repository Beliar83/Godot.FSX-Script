namespace Godot.FSXScript.MsBuild

open System
open System.IO
open System.Text
open FsxScript.Parser
open Godot.FSharp
open Godot.FSharp.Variant
open Microsoft.Build.Framework
open Microsoft.Build.Utilities

type FsxToGodotSharp() as this =
    inherit Task()
   
    let isSinglePrecision (field : ObjectGenerator.Field) = field.OfTypeName.ToString() = "System.Single"
    let getConversionForPrecision field = if isSinglePrecision field then "Single" else "Double"
    let systemTypePrefixLength = "System.".Length
    
    let getConversionForField valueName (field : ObjectGenerator.Field) =
        match getConversionToDotnetForGodotSharp Console.Error.WriteLine field.OfType with
        | Simple methodName -> $"{valueName}.{methodName}()"
        | Nil -> "null"
        | Int -> $"{valueName}.As{field.OfTypeName.ToString().Remove(0, systemTypePrefixLength)}()"
        | Float -> $"{valueName}.As{getConversionForPrecision field}()"
        | Object -> $"{valueName}.AsGodotObject() as {field.OfTypeName}"
        
    [<Required>]member val ScriptFiles = Array.empty<ITaskItem> with get, set
    [<Required>]member val GodotSharpPath = "" with get, set
    [<Required>]member val FSharpCompilerPath = "" with get, set
    [<Required>]member val Namespace = "" with get, set
    member val GeneratedPath = "Generated" with get, set    
    
    [<Output>]member val GeneratedFiles = Array.empty<string> with get, set    
    
    [<Output>]member val Reference = "" with get, set  
    
    override _.Execute() =
        Environment.SetEnvironmentVariable("FSHARP_COMPILER_BIN", this.FSharpCompilerPath)
        
        let otherFlags =
            [|"--targetprofile:netcore"|]
            |> Array.append [|$"-r:{this.GodotSharpPath}"|]
        
        if not <| Directory.Exists this.GeneratedPath then
            Directory.CreateDirectory(this.GeneratedPath) |> ignore
        
        for item in this.ScriptFiles do            
            let scriptPath = item.GetMetadata("FullPath")
            let _, info, _ = ParseScript(File.ReadAllText(scriptPath), scriptPath, otherFlags, Console.Error.WriteLine)

            let info = 
                match info with
                | None -> failwith "todo"
                | Some value -> value

            let moduleName = info.ModuleNameToOpen
            let moduleName =
                if moduleName.StartsWith("global.") then                    
                    String.concat "::" ["global"; moduleName.Substring("global.".Length)]                    
                else
                    moduleName

            let addExportedProperties (builder: StringBuilder) =
                
                let appendLines (lines: string list) (builder: StringBuilder) =
                    lines |> List.fold (fun (builder : StringBuilder) -> builder.AppendLine) builder

                let generateFunctions (properties: ObjectGenerator.Field list) =
                    properties
                    |> List.fold (fun (nameCalls, propertyListCalls, getCalls, setCalls) property ->
                        let namePropertyName = $"@{property.Name.Substring(0, 1).ToUpper()}{property.Name.Substring(1)}";
                        let nameCallsForField (builder : StringBuilder) = builder.AppendLine($"\tstatic StringName {namePropertyName} = new StringName(\"{property.Name}\");")
                        let propertyListCallsForField (builder : StringBuilder) =
                            builder
                            |> appendLines
                                [
                                    "\t\tproperties.Add(new Godot.Collections.Dictionary()"
                                    "\t\t{"
                                    $"\t\t\t{{ \"name\", \"{property.Name}\" }},"
                                    $"\t\t\t{{ \"type\", (int)Variant.Type.{property.OfType} }},"
                                    $"\t\t\t{{ \"hint\", (int)PropertyHint.{property.PropertyHint} }},"
                                    $"\t\t\t{{ \"hint_string\", \"{property.HintText}\" }},"
                                    "\t\t});"
                                ]
                        let getCallsForField (builder : StringBuilder) =
                            builder
                            |> appendLines
                                [
                                    $"\t\tif (property == {namePropertyName})"
                                    "\t\t{"
                                    $"\t\t\treturn Variant.CreateFrom(state.@{property.Name});"
                                    "\t\t}"
                                ]
                                
                        let conversion = getConversionForField "value" property;
                        
                        let setCallsForField (builder : StringBuilder) =
                            builder
                            |> appendLines
                                [
                                    $"\t\tif (property == {namePropertyName})"
                                    "\t\t{"
                                    $"\t\t\tstate = {moduleName}.__set(state, {namePropertyName}, {conversion});"
                                    "\t\t\treturn true;"
                                    "\t\t}"
                                ]                            
                        (nameCallsForField :: nameCalls,  propertyListCallsForField :: propertyListCalls, getCallsForField :: getCalls, setCallsForField :: setCalls)
                    ) ([], [], [], [])
                    |> fun (nameCalls, propertyListCalls, getCalls, setCalls) -> (List.rev nameCalls, List.rev propertyListCalls, List.rev getCalls, List.rev setCalls)

                let nameCalls, propertyListFunctions, getFunctions, setFunctions =
                    generateFunctions info.StateToGenerate.ExportedFields

                let appendFromFunction functions (builder: StringBuilder) =
                    functions
                    |> List.fold (fun (builder : StringBuilder) f -> f builder) builder

                let appendNames = appendFromFunction <| nameCalls
                let appendPropertyList = appendFromFunction <| propertyListFunctions
                let appendGets = appendFromFunction <| getFunctions
                let appendSets  = appendFromFunction <| setFunctions
                
                let builder = appendNames builder
                
                let builder =
                    builder
                    |> appendLines
                        [ "\tpublic override Godot.Collections.Array<Godot.Collections.Dictionary> _GetPropertyList()"
                          "\t{"
                          "\t\tvar properties = new Godot. Collections. Array<Godot. Collections. Dictionary>();"
                        ]
                    |> appendPropertyList
                    |> appendLines
                        [
                            "\t\treturn properties;"
                            "\t}"
                        ]

                let builder =
                    builder
                    |> appendLines
                        [ "\tpublic override Variant _Get(StringName property)"
                          "\t{"
                        ]
                    |> appendGets
                    |> _.AppendLine("\t\treturn default;")
                    |> appendLines [ "\t}"]

                let builder =
                    builder
                    |> appendLines
                        [ "\tpublic override bool _Set(StringName property, Variant value)"
                          "\t{" ]
                    |> appendSets
                    |> _.AppendLine("\t\treturn false;")
                    |> appendLines [ "\t}"]

                builder 
            
            let addMethods (builder : StringBuilder) =
                info.Methods
                |> List.fold (fun (builder : StringBuilder) method ->
                        let returnParameter =
                            match method.ReturnParameter with
                            | None -> "void"
                            | Some value -> value.OfTypeName

                        let addCall(builder : StringBuilder) =
                            let methodParams =
                                [["this"]; method.MethodParams |> List.map _.Name; ["state"]]
                                |> List.concat
                                
                            
                            builder
                                .Append($"{moduleName}.{method.MethodName}(")
                                    .AppendJoin(", ", methodParams)
                                    .AppendLine(");")
                                                
                        let addCallAndReturn(builder : StringBuilder) =
                            match method.ReturnParameter with
                            | None ->
                                builder
                                    .Append("\t\tstate = ") |> addCall
                            | Some _ ->
                                builder
                                    |> _.Append("\t\tvar retVal = ") |> addCall
                                    |> _.AppendLine("\t\tstate = retVal.Item1;")
                                    |> _.AppendLine("\t\treturn retVal.Item2;")
                            
                        builder
                            |> _.Append($"\tprivate {returnParameter} {method.MethodName}(")
                                |> _.AppendJoin(", ", method.MethodParams |> List.map (fun argument -> $"{argument.OfTypeName} {argument.Name}"))
                                |> _.AppendLine(")")
                            |> _.AppendLine("\t{")
                            |> addCallAndReturn
                            |> _.AppendLine("\t}")
                    ) builder
            
            let sourceBuilder =             
                StringBuilder()
                |> _.AppendLine("using Godot;")
                |> _.AppendLine()
                |> _.AppendLine($"namespace {this.Namespace};")
                |> _.AppendLine()
                |> _.AppendLine($"public partial class {info.Name} : {info.Extending}")
                |> _.AppendLine("{")
                |> _.AppendLine($"\tprivate {moduleName}.State state = {moduleName}.__get_default_state();")
                |> _.AppendLine()
                |> addExportedProperties                
                |> _.AppendLine()
                |> addMethods
                |> _.AppendLine("}")
                |> _.AppendLine()                                  
            
            let csFilePath = Path.Join(this.GeneratedPath, $"{info.Name}.cs")
            let csFile = new StreamWriter(File.Create(csFilePath))
            csFile.Write("""// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>""")
            csFile.WriteLine()
            csFile.WriteLine("#if !TOOLS")
            csFile.Write(sourceBuilder.Replace("\t", "    "))
            csFile.WriteLine("#endif")
            csFile.Close()
            
            this.GeneratedFiles <-
                this.GeneratedFiles
                |> Array.append [|csFilePath|]
            
            
            // session.
        true
