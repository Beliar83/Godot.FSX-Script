namespace FSXScriptInterpreter

open System.IO
open Godot
open Godot.Bridge
open Godot.Collections

type FsxScriptResourceFormatLoader() as this =
    inherit ResourceFormatLoader()

    static member extensions = [| "fsx" |]

    static member recognizedExtensions =
        new PackedStringArray(FsxScriptResourceFormatLoader.extensions)

    static member typeName = new StringName("Script")

    static member BindMethods(context: ClassDBRegistrationContext) =
        context.BindConstructor(fun () -> new FsxScriptResourceFormatLoader())

    override _._GetRecognizedExtensions() =
        FsxScriptResourceFormatLoader.recognizedExtensions

    override _._RecognizePath(path, _type) =
        this._HandlesType _type
        && FsxScriptResourceFormatLoader.extensions
           |> Array.contains (Path.GetExtension(path).Substring(1))

    override _._HandlesType(_type) =
        _type.IsEmpty
        || _type = FsxScriptResourceFormatLoader.typeName

    override _._GetResourceType(path) =
        if path.EndsWith ".fsx" then
            "Script"
        else
            ""

    override this._Load(path, originalPath, useSubThreads, cacheMode) =
        GD.Print("_Load")

        let file =
            FileAccess.Open(path, FileAccess.ModeFlags.Read)

        let code = file.GetAsText()
        let script = new FsxScript()
        script.SetPath(path)
        script.SetSourceCode(code)
        Variant.CreateFrom(script)
