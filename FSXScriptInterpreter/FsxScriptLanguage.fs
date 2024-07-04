namespace FSXScriptInterpreter

open Godot
open Godot.Bridge
open Godot.Collections

type FsxScriptLanguage() =
    inherit ScriptLanguageExtension()
    
    static member BindMethods(context: ClassDBRegistrationContext) =
        context.BindConstructor(fun () -> new FsxScriptLanguage())
    
    override this._Init() =
        ()
    override _._CreateScript() =
        GD.Print("CreateScript")
        new FsxScript()
    override _._GetName() =
        "FsxScriptLanguage"
    override this._GetType() =
        GD.Print("GetType")
        base._GetType()
    override this._GetExtension() =
        "fsx"
    override this._Finish() =
        GD.Print("Finish")
        base._Finish()
    override this._GetReservedWords() =
        GD.Print("GetReservedWords")
        new PackedStringArray()
    override this._IsControlFlowKeyword(keyword) =
        GD.Print("IsControlFlowKeyword")
        base._IsControlFlowKeyword(keyword)
    override this._GetCommentDelimiters() =
        GD.Print("GetCommentDelimiters")
        new PackedStringArray()
    override this._GetDocCommentDelimiters() =
        GD.Print("GetDocCommentDelimiters")
        new PackedStringArray()
    override this._GetStringDelimiters() =
        GD.Print("GetStringDelimiters")
        new PackedStringArray()
    override this._MakeTemplate(template, className, baseClassName) =
        let code =
            $"""module {className}

//This sets the godot class to inherit from
type Base = {baseClassName}

//Define fields in this type. Use [Export] to mark exported fields.
type State = struct end

let _process(self : Base) (delta: float) =
    ()"""
        let script = new FsxScript()
        script.SetSourceCode(code)
        script
        
    override this._GetBuiltInTemplates(object) =
        GD.Print("GetBuiltInTemplates")
        new GodotArray<GodotDictionary>()
    override this._IsUsingTemplates() =
        GD.Print("IsUsingTemplates")
        base._IsUsingTemplates()
    override this._Validate(script, path, validateFunctions, validateErrors, validateWarnings, validateSafeLines) =
        GD.Print("date")
        new GodotDictionary()
    override this._ValidatePath(path) =
        GD.Print("ValidatePath")
        base._ValidatePath(path)
    override this._HasNamedClasses() =
        GD.Print("HasNamedClasses")
        base._HasNamedClasses()
    override this._SupportsBuiltinMode() =
        GD.Print("SupportsBuiltinMode")
        base._SupportsBuiltinMode()
    override this._SupportsDocumentation() =
        GD.Print("SupportsDocumentation")
        base._SupportsDocumentation()
    override this._CanInheritFromFile() =
        GD.Print("CanInheritFromFile")
        base._CanInheritFromFile()
    override this._FindFunction(_function, code) =
        GD.Print("FindFunction")
        base._FindFunction(_function, code)
    override this._MakeFunction(className, functionName, functionArgs) =
        GD.Print("MakeFunction")
        base._MakeFunction(className, functionName, functionArgs)
    override this._CanMakeFunction() =
        GD.Print("CanMakeFunction")
        base._CanMakeFunction()
    override this._OpenInExternalEditor(script, line, column) =
        GD.Print("OpenInExternalEditor")
        base._OpenInExternalEditor(script, line, column)
    override this._OverridesExternalEditor() =
        GD.Print("OverridesExternalEditor")
        base._OverridesExternalEditor()
    override this._PreferredFileNameCasing() =
        GD.Print("PreferredFileNameCasing")
        base._PreferredFileNameCasing()
    override this._CompleteCode(code, path, owner) =
        GD.Print("CompleteCode")
        new GodotDictionary()
    override this._LookupCode(code, symbol, path, owner) =
        GD.Print("LookupCode")
        new GodotDictionary()
    override this._AutoIndentCode(code, fromLine, toLine) =
        GD.Print("AutoIndentCode")
        base._AutoIndentCode(code, fromLine, toLine)
    override this._AddGlobalConstant(name, value) =
        GD.Print("AddGlobalConstant")
        base._AddGlobalConstant(name, value)
    override this._AddNamedGlobalConstant(name, value) =
        GD.Print("AddNamedGlobalConstant")
        base._AddNamedGlobalConstant(name, value)
    override this._RemoveNamedGlobalConstant(name) =
        GD.Print("RemoveNamedGlobalConstant")
        base._RemoveNamedGlobalConstant(name)
    override this._ReloadAllScripts() =
        GD.Print("ReloadAllScripts")
        base._ReloadAllScripts()
    override this._ReloadToolScript(script, softReload) =
        GD.Print("ReloadToolScript")
        base._ReloadToolScript(script, softReload)
    override this._GetRecognizedExtensions() =
        new PackedStringArray([|"fsx"|])        
    override this._GetPublicFunctions() =
        GD.Print("GetPublicFunctions")
        new GodotArray<GodotDictionary>()
    override this._GetPublicConstants() =
        GD.Print("GetPublicConstants")
        new GodotDictionary()
    override this._GetPublicAnnotations() =
        GD.Print("GetPublicAnnotations")
        new GodotArray<GodotDictionary>()
    override this._HandlesGlobalClassType(_type) =
        GD.Print("HandlesGlobalClassType")
        base._HandlesGlobalClassType(_type)
    override this._GetGlobalClassName(path) =
        GD.Print("_GetGlobalClassName")
        let dict = new GodotDictionary()
        dict["name"] <- "Test"
        dict["base_type"] <- "Node"
        dict
    override this._ThreadEnter() = ()
    override this._ThreadExit() = ()
    override this._Frame() = base._Frame()