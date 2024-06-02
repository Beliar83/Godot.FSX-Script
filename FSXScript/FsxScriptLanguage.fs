namespace FSXScript

open System.Collections.Generic
open Godot
open Godot.Bridge
open Godot.Collections

type FsxScriptLanguage() as self =
    inherit ScriptLanguageExtension()
    
    static member BindMethods(context: ClassDBRegistrationContext) =
        // context.BindMethod(
        //     new StringName(nameof (Unchecked.defaultof<Summator>.Add)),
        //     new ParameterInfo(new StringName("value"), VariantType.Int, VariantTypeMetadata.Int32, 1),
        //     fun (instance: Summator) (value: int) -> instance.Add(value)
        // )
        
        context.BindConstructor(fun () -> new FsxScript())
        
        context.BindVirtualMethodOverride<FsxScriptLanguage, int>(
            new StringName(nameof (Unchecked.defaultof<FsxScriptLanguage>._preferred_file_name_casing)),
            fun (instance: FsxScriptLanguage)-> instance._preferred_file_name_casing()
        )
    
    override _._GetName() = "FsxScriptLanguage"   
    
    override _._GetType() = nameof(FsxScript)
    
    override _._GetExtension() = "fsx"
    
    member _._preferred_file_name_casing() =
        2
    
    override _._GetReservedWords() =
        new PackedStringArray()        
    
    override _._GetCommentDelimiters() =
        new PackedStringArray()

    override _._GetDocCommentDelimiters() =
        new PackedStringArray()

    override _._GetStringDelimiters() =
        new PackedStringArray()
        
    override _._MakeTemplate(template, className, baseClassName) =
        new FsxScript()
        
    override _._GetBuiltInTemplates(object) =
        new GodotArray<GodotDictionary>()   

    override _._Validate(script, path, validateFunctions, validateErrors, validateWarnings, validateSafeLines) =
        let dict = new GodotDictionary()

        dict.Add("valid", true)
        
        dict
    
    override _._ValidatePath(path) =
        // TODO
        ""
        
    override _._CreateScript() =
        new FsxScript()
        
    override _._HasNamedClasses() = true
    
    override _._MakeFunction(className, functionName, functionArgs) =
        "// TODO: _MakeFunction"  
    
    override _._CompleteCode(code, path, owner) =
        new GodotDictionary()
    
    override _._LookupCode(code, symbol, path, owner) =
        new GodotDictionary()
    
    override _._AutoIndentCode(code, fromLine, toLine) =
        // TODO
        code
    
    override _._DebugGetError() =
        ""
    
    override _._DebugGetStackLevelFunction(level) =
        ""
    
    override _._DebugGetStackLevelMembers(level, maxSubItems, maxDepth) =
        new GodotDictionary()
    
    override _._DebugGetStackLevelInstance(level) =
        (new nativeint(0)).ToPointer()
    
    override _._DebugGetGlobals(maxSubitems, maxDepth) =
        new GodotDictionary()
    
    override _._DebugParseStackLevelExpression(level, expression, maxSubItems, maxDepth) =
        ""        
        
    override _._DebugGetCurrentStackInfo() =
        new GodotArray<GodotDictionary>()
    
    override _._GetRecognizedExtensions() =
        new PackedStringArray([|"fsx"|])
    
    override _._GetPublicFunctions() =
        new GodotArray<GodotDictionary>()
        
    override _._GetPublicConstants() =
        new GodotDictionary()
        
    override _._GetPublicAnnotations() =
        new GodotArray<GodotDictionary>()
        
    override _._GetGlobalClassName(path) =
        new GodotDictionary()      
    
    
    // override _._
    
    
    
    