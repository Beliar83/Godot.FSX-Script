namespace FSXScript

open Godot
open Godot.Bridge
open Godot.Collections

type FsxScriptResourceFormatSaver() as self =
    inherit ResourceFormatSaver()
    
    override _._GetRecognizedExtensions(resource) =
        let array = new PackedStringArray()
        
        if resource.IsClass(nameof(FsxScript)) then
            array.Add("fsx")
        
        array
    
    override _._Recognize(resource) =
        printf($"{resource}")
        resource.IsClass(nameof(FsxScript))
            
    override _._RecognizePath(resource, path) =
        printf($"{resource} {path}")
        path.EndsWith(".fsx") && self._Recognize(resource)
    
    override _._Save(resource, path, flags) =
        let file = FileAccess.Open(path, FileAccess.ModeFlags.Write)
        resource.SetPath(path)
        let script = resource :?> Script
        file.StoreString(script.GetSourceCode())
        file.Close()        
        Error.Ok
        
    override _._SetUid(path, uid) =
        Error.Unconfigured
        
    static member BindMethods(context: ClassDBRegistrationContext) =
        context.BindConstructor(fun () -> new ResourceFormatSaver())