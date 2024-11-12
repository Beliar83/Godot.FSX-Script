namespace FSXScriptInterpreter

open FSXScriptInterpreter.FsxScriptInstance
open Godot
open Godot.Bridge
open Godot.Collections
open LspService

type FsxScript() as this =
    inherit ScriptExtension()
    let mutable sourceCode: string = null
    let session: ScriptSession = ScriptSession.CreateScriptSession(this)

    static member val LanguageName = new StringName("FsxScriptLanguage")
    static member val ClassName = new StringName(nameof FsxScript)

    static member BindMethods(context: ClassDBRegistrationContext) =
        context.BindConstructor(fun () -> new FsxScript())

    override _._InstanceCreate(forObject: GodotObject) =
        Godot.Extensions.Script.CreateInstance<FsxScriptInstance>(forObject, this)

    override _._GetLanguage() =
        GD.Print("_GetLanguage")
        Engine.Singleton.GetSingleton(FsxScript.LanguageName) :?> ScriptLanguage

    override _._CanInstantiate() =
        GD.Print("CanInstantiate")
        false

    override _._EditorCanReloadFromFile() =
        GD.Print("CanInstantiate")
        false

    override _._GetBaseScript() =
        GD.Print("GetBaseScript")
        null

    override _._GetClassIconPath() =
        GD.Print("GetClassIconPath")
        null

    override _._GetConstants() =
        GD.Print("GetConstants")
        new GodotDictionary()

    override _._GetDocumentation() =
        GD.Print("GetDocumentation")
        new GodotArray<GodotDictionary>()

    override _._GetGlobalName() =
        GD.Print("GetGlobalName")
        new StringName(session.GetClassName())

    override _._GetInstanceBaseType() =
        GD.Print("GetInstanceBaseType")
        new StringName("Object")

    override _._GetMemberLine(_member) =
        GD.Print("GetMemberLine")
        0

    override _._GetMembers() =
        GD.Print("GetMembers")
        new GodotArray<StringName>()

    override _._GetMethodInfo(method) =
        GD.Print("GetMethodInfo")
        new GodotDictionary()

    override _._GetPropertyDefaultValue(property) =
        GD.Print("GetPropertyDefaultValue")
        new Variant()

    override _._GetRpcConfig() =
        GD.Print("GetRpcConfig")
        new Variant()

    override _._GetScriptMethodArgumentCount(method) =
        GD.Print("GetScriptMethodArgumentCount")
        new Variant()

    override _._GetScriptMethodList() =
        GD.Print("GetScriptMethodList")
        new GodotArray<GodotDictionary>()

    override _._GetScriptPropertyList() =
        GD.Print("GetScriptPropertyList")
        new GodotArray<GodotDictionary>()

    override _._GetScriptSignalList() =
        GD.Print("GetScriptSignalList")
        new GodotArray<GodotDictionary>()

    override _._PlaceholderErased(placeholder) =
        GD.Print("PlaceholderErased")
        ()

    override _._InheritsScript(script) =
        GD.Print("InheritsScript")
        false

    override _._PlaceholderInstanceCreate(forObject) =
        Godot.Extensions.Script.CreateInstance<FsxScriptInstance>(forObject, this, true)

    override _._InstanceHas(object) =
        GD.Print("_InstanceHas")
        false

    override _._HasSourceCode() = sourceCode <> null

    override _._GetSourceCode() = sourceCode

    override _._SetSourceCode(code) =
        sourceCode <- code
        session.NotifyScriptChange()

    override _._Reload(keepState) =
        GD.Print("_Reload")
        Error.Unavailable

    override _._HasMethod(method) =
        GD.Print("_HasMethod")
        false

    override _._HasStaticMethod(method) =
        GD.Print("_HasStaticMethod")
        false

    override _._IsTool() =
        GD.Print("_IsTool")
        false

    override _._IsValid() =
        GD.Print("_IsValid")
        false

    override _._IsAbstract() =
        GD.Print("_IsAbstract")
        false

    override _._HasScriptSignal(signal) =
        GD.Print("_HasScriptSignal")
        false

    override _._HasPropertyDefaultValue(property) =
        GD.Print("_HasPropertyDefaultValue")
        false

    override _._UpdateExports() =
        GD.Print("_UpdateExports")
        ()

    override _._IsPlaceholderFallbackEnabled() =
        GD.Print("_IsPlaceholderFallbackEnabled")
        false

    override this.Dispose(disposing) =
        session.NotifyScriptClose()
        base.Dispose(disposing)
