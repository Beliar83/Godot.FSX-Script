module FsxScript.Parser

open FSharp.Compiler.CodeAnalysis
open FSharp.Compiler.Text
open Godot.FSharp.ObjectGenerator

let ParseScript (scriptCode: string, scriptPath: string, otherFlags, printError: string -> unit) =
    let checker =
        FSharpChecker.Create(keepAssemblyContents = true)

    let parseResults, parseAnswer =
        let scriptCode = scriptCode |> SourceText.ofString

        let options, _ =
            checker.GetProjectOptionsFromScript(scriptPath, scriptCode, otherFlags = otherFlags)
            |> Async.RunSynchronously

        checker.ParseAndCheckFileInProject(scriptPath, 0, scriptCode, options)
        |> Async.RunSynchronously

    let answer =
        match parseAnswer with
        | FSharpCheckFileAnswer.Aborted -> None
        | FSharpCheckFileAnswer.Succeeded checkFileResults -> Some(checkFileResults)

    let file =
        match answer with
        | None -> None
        | Some value -> value.ImplementationFile

    let info =
        match file with
        | None -> None
        | Some file ->
            match generateInfo file with
            | Error messages ->
                for message in messages do
                    printError $"Error parsing {scriptPath}: {message}"

                None
            | Ok info -> Some(info)

    (parseResults, info, answer)
