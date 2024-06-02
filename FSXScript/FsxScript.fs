namespace FSXScript

open Godot
open Godot.Bridge

type FsxScript() as self =
    inherit ScriptExtension()

    override _._InstanceCreate(object) =
        nativeint(0).ToPointer()
        
    static member BindMethods(context: ClassDBRegistrationContext) =
        context.BindConstructor(fun () -> new FsxScript())


    