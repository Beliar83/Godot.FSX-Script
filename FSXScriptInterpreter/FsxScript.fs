namespace FSXScriptInterpreter

open FSXScriptInterpreter.FsxScriptInstance
open Godot
open Godot.Bridge

type FsxScript() as this =
    inherit ScriptExtension()
    let mutable sourceCode : string = null
    
    static member val LanguageName = new StringName("FsxScriptLanguage")

    static member BindMethods(context: ClassDBRegistrationContext) =
        context.BindConstructor(fun () -> new FsxScript())
    override _._InstanceCreate(forObject : GodotObject) =
        Godot.Extensions.Script.CreateInstance<FsxScriptInstance>(forObject, this)
    override _._GetLanguage() =
        GD.Print("_GetLanguage")
        Engine.Singleton.GetSingleton(FsxScript.LanguageName) :?> ScriptLanguage
    override _._CanInstantiate() =
        GD.Print("CanInstantiate")
        base._CanInstantiate()    
    override _._EditorCanReloadFromFile() =
        GD.Print("CanInstantiate")
        base._EditorCanReloadFromFile()    
    override _._GetBaseScript() =
        GD.Print("GetBaseScript")
        base._GetBaseScript()    
    override _._GetClassIconPath() =
        GD.Print("GetClassIconPath")
        base._GetClassIconPath()    
    override _._GetConstants() =
        GD.Print("GetConstants")
        base._GetConstants()    
    override _._GetDocumentation() =
        GD.Print("GetDocumentation")
        base._GetDocumentation()
    override _._GetGlobalName() =
        GD.Print("GetGlobalName")
        base._GetGlobalName()    
    override _._GetInstanceBaseType() =
        GD.Print("GetInstanceBaseType")
        base._GetInstanceBaseType()            
    override _._GetMemberLine(_member) =
        GD.Print("GetMemberLine")
        base._GetMemberLine(_member)
    override _._GetMembers() =
        GD.Print("GetMembers")
        base._GetMembers()
    override _._GetMethodInfo(method) =
        GD.Print("GetMethodInfo")
        base._GetMethodInfo(method)                
    override _._GetPropertyDefaultValue(property) =
        GD.Print("GetPropertyDefaultValue")
        base._GetPropertyDefaultValue(property)
    override _._GetRpcConfig() =
        GD.Print("GetRpcConfig")
        base._GetRpcConfig()        
    override _._GetScriptMethodArgumentCount(method) =
        GD.Print("GetScriptMethodArgumentCount")
        base._GetScriptMethodArgumentCount(method)
    override _._GetScriptMethodList() =
        GD.Print("GetScriptMethodList")
        base._GetScriptMethodList()
    override _._GetScriptPropertyList() =
        GD.Print("GetScriptPropertyList")
        base._GetScriptPropertyList()
    override _._GetScriptSignalList() =
        GD.Print("GetScriptSignalList")
        base._GetScriptSignalList()  
    override _._PlaceholderErased(placeholder) =
        GD.Print("PlaceholderErased")
        base._PlaceholderErased(placeholder)
    override _._InheritsScript(script) =
        GD.Print("InheritsScript")
        base._InheritsScript(script)
    override _._PlaceholderInstanceCreate(forObject) =
        Godot.Extensions.Script.CreateInstance<FsxScriptInstance>(forObject, this, true)        
    override _._InstanceHas(object) =
        GD.Print("_InstanceHas")
        base._InstanceHas(object)
    override _._HasSourceCode() =
        sourceCode <> null
    override _._GetSourceCode() =
        sourceCode        
    override _._SetSourceCode(code) =
        sourceCode <- code
    override _._Reload(keepState) =
        GD.Print("_Reload")
        base._Reload(keepState)
    override _._HasMethod(method) =
        GD.Print("_HasMethod")
        base._HasMethod(method)
    override _._HasStaticMethod(method) =
        GD.Print("_HasStaticMethod")
        base._HasStaticMethod(method)
    override _._IsTool() =
        GD.Print("_IsTool")
        base._IsTool()
    override _._IsValid() =
        GD.Print("_IsValid")
        base._IsValid()
    override _._IsAbstract() =
        GD.Print("_IsAbstract")
        base._IsAbstract()
    override _._HasScriptSignal(signal) =
        GD.Print("_HasScriptSignal")
        base._HasScriptSignal(signal)
    override _._HasPropertyDefaultValue(property) =
        GD.Print("_HasPropertyDefaultValue")
        base._HasPropertyDefaultValue(property)
    override _._UpdateExports() =
        GD.Print("_UpdateExports")
        base._UpdateExports()
    override _._IsPlaceholderFallbackEnabled() =
        GD.Print("_IsPlaceholderFallbackEnabled")
        base._IsPlaceholderFallbackEnabled()
