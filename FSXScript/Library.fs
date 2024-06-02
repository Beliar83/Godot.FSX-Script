namespace GDExtensionFsxScript

open System
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open FSXScript
open Godot
open Godot.Bridge

module Library =
    [<assembly: DisableRuntimeMarshalling>]
    do ()

    type FsxScriptLibraryInitFunc = delegate of nativeint * nativeint * nativeint -> bool
    
    let mutable language : Option<FsxScriptLanguage> = None
    let mutable saver : Option<FsxScriptResourceFormatSaver> = None

    let InitializeFsxScriptTypes (level: InitializationLevel) : unit =
        match level with
        | InitializationLevel.Scene ->
            ClassDB.RegisterClass<FsxScript>(Action<ClassDBRegistrationContext>(FsxScript.BindMethods))
            ClassDB.RegisterClass<FsxScriptLanguage>(Action<ClassDBRegistrationContext>(FsxScriptLanguage.BindMethods))
            ClassDB.RegisterClass<FsxScriptResourceFormatSaver>(Action<ClassDBRegistrationContext>(FsxScriptResourceFormatSaver.BindMethods))
            
            language <- Some(new FsxScriptLanguage())
            Engine.Singleton.RegisterScriptLanguage(language.Value) |> ignore
            saver <- Some(new FsxScriptResourceFormatSaver())
            ResourceSaver.Singleton.AddResourceFormatSaver(saver.Value)
            printfn("InitializationLevel.Scene done")
        | _ -> ()
            

    let DeinitializeFsxScriptTypes (level: InitializationLevel) : unit =
        if level <> InitializationLevel.Scene then            
            ()
        else
            ()  
    
    
    let FsxScriptLibraryInit (getProcAddress: nativeint, library: nativeint, initialization: nativeint) : bool =
        Console.WriteLine "FsxScriptLibraryInit From F#"
        Console.WriteLine $"{getProcAddress}, {library}, {initialization}"
        GodotBridge.Initialize(
            getProcAddress,
            library,
            initialization,
            fun config ->
                config.SetMinimumLibraryInitializationLevel(InitializationLevel.Scene)
                config.RegisterInitializer(InitializeFsxScriptTypes)
                config.RegisterTerminator(DeinitializeFsxScriptTypes)
        )
        
        true
