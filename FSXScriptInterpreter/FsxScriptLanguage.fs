namespace FSXScriptInterpreter

open Godot
open Godot.Bridge
open Godot.Collections

type FsxScriptLanguage() =
    inherit ScriptLanguageExtension()

    static member BindMethods(context: ClassDBRegistrationContext) =
        context.BindConstructor(fun () -> new FsxScriptLanguage())

    override this._Init() = ()

    override _._CreateScript() =
        GD.Print("CreateScript")
        new FsxScript()

    override _._GetName() = "FsxScriptLanguage"
    override this._GetType() = "FsxScript"
    override this._GetExtension() = "fsx"

    override this._Finish() =
        GD.Print("Finish")
        ()

    override this._GetReservedWords() =
        GD.Print("GetReservedWords")
        new PackedStringArray()

    override this._IsControlFlowKeyword(keyword) =
        GD.Print("IsControlFlowKeyword")
        false

    override this._GetCommentDelimiters() = new PackedStringArray([ "//" ])

    override this._GetDocCommentDelimiters() =
        GD.Print("GetDocCommentDelimiters")
        new PackedStringArray()

    override this._GetStringDelimiters() =
        let list = new PackedStringArray()
        list.Add("\" \"")
        list.Add("' '")
        list.Add("@\" \"")
        list

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
        false

    override this._Validate(script, path, validateFunctions, validateErrors, validateWarnings, validateSafeLines) =
        GD.Print("date")
        new GodotDictionary()

    override this._ValidatePath(path) =
        GD.Print("ValidatePath")
        path

    override this._HasNamedClasses() =
        GD.Print("HasNamedClasses")
        false

    override this._SupportsBuiltinMode() =
        GD.Print("SupportsBuiltinMode")
        false

    override this._SupportsDocumentation() =
        GD.Print("SupportsDocumentation")
        false

    override this._CanInheritFromFile() =
        GD.Print("CanInheritFromFile")
        false

    override this._FindFunction(_function, code) =
        GD.Print("FindFunction")
        0

    override this._MakeFunction(className, functionName, functionArgs) =
        GD.Print("MakeFunction")
        ""

    override this._CanMakeFunction() =
        GD.Print("CanMakeFunction")
        false

    override this._OpenInExternalEditor(script, line, column) =
        GD.Print("OpenInExternalEditor")
        Error.Unavailable

    override this._OverridesExternalEditor() =
        GD.Print("OverridesExternalEditor")
        false

    override this._PreferredFileNameCasing() =
        GD.Print("PreferredFileNameCasing")
        Godot.ScriptLanguage.ScriptNameCasing.SnakeCase

    override this._CompleteCode(code, path, owner) =
        GD.Print("CompleteCode")
        new GodotDictionary()

    override this._LookupCode(code, symbol, path, owner) =
        GD.Print("LookupCode")
        new GodotDictionary()

    override this._AutoIndentCode(code, fromLine, toLine) =
        GD.Print("AutoIndentCode")
        code

    override this._AddGlobalConstant(name, value) =
        GD.Print("AddGlobalConstant")
        ()

    override this._AddNamedGlobalConstant(name, value) =
        GD.Print("AddNamedGlobalConstant")
        ()

    override this._RemoveNamedGlobalConstant(name) =
        GD.Print("RemoveNamedGlobalConstant")
        ()

    override this._ReloadAllScripts() =
        GD.Print("ReloadAllScripts")
        ()

    override this._ReloadToolScript(script, softReload) =
        GD.Print("ReloadToolScript")
        ()

    override this._GetRecognizedExtensions() = new PackedStringArray([| "fsx" |])

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
        false

    override this._GetGlobalClassName(path) =
        GD.Print("_GetGlobalClassName")
        let dict = new GodotDictionary()
        dict["name"] <- "Test"
        dict["base_type"] <- "Node"
        dict

    override this._ThreadEnter() = ()
    override this._ThreadExit() = ()
    override this._Frame() = ()
