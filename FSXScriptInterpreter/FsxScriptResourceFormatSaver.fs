namespace FSXScriptInterpreter

open Godot
open Godot.Collections
open Godot.Bridge

type FsxScriptResourceFormatSaver() as this =
    inherit ResourceFormatSaver()

    static member BindMethods(context: ClassDBRegistrationContext) =
        context.BindConstructor(fun () -> new FsxScriptResourceFormatSaver())
    
    override _._Save(resource, path, flags) =
        let script = resource :?> FsxScript
        let file = FileAccess.Open(path, FileAccess.ModeFlags.WriteRead)
        if file = null then
            let error = FileAccess.GetOpenError()
            error
        else
            file.StoreString(script.GetSourceCode())
            if file.GetError() <> Error.Ok then
                file.Close()
                Error.CantCreate
            else
                file.Close()
                Error.Ok        
        
    override _._Recognize(resource) = resource :? FsxScript
    override _._GetRecognizedExtensions(resource) =
        if this._Recognize(resource) then
            new PackedStringArray([|"fsx"|])
        else
            new PackedStringArray()
    override th_._RecognizePath(resource, path) =
        if this._Recognize(resource) then
            // TODO: Check code
            true
        else
            false