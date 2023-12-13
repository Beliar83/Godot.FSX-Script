using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using CaseExtensions;
using SharpGenerator.Documentation;

namespace SharpGenerator;

public class Convert
{
    private const string DefaultBuiltinCsConstructorPattern = "\t\tpublic {0} : base({1}) {{}}";
    private readonly HashSet<string> objectTypes = new() { "Variant" };

    private readonly HashSet<string> builtinObjectTypes = new() {
        "Array",
        "Callable",
        "Dictionary",
        "PackedByteArray",
        "PackedColorArray",
        "PackedFloat32Array",
        "PackedFloat64Array",
        "PackedInt32Array",
        "PackedInt64Array",
        "PackedStringArray",
        "PackedVector2Array",
        "PackedVector3Array",
        "NodePath",
        "Signal",
        "StringName",
    };

    private readonly Api api;
    private readonly XmlSerializer classXml = new(typeof(Class));
    private readonly XmlSerializer builtinXml = new(typeof(BuiltinClass));
    private readonly string csDir;
    private readonly string rsGeneratedDir;
    private readonly string? docDir;
    private readonly string configName;
    
    public Convert(Api api, string csDir, string rsGeneratedDir, string docDir)
    {
        this.api = api;
        this.csDir = csDir;
        this.rsGeneratedDir = rsGeneratedDir;
        this.docDir = docDir;
        var rustConfigFile = new StreamWriter(Path.Join(this.rsGeneratedDir, "config.rs"));
        rustConfigFile.WriteLine("use phf::phf_map;");

        foreach (Api.BuiltinClassSizesOfConfig classSizes in api.BuiltinClassSizes)
        {
            string[] configArray = classSizes.BuildConfiguration.Split("_").ToArray();
            bool doublePrecision = configArray[0] == "double";
            string targetPointerWidth = configArray[1];
            rustConfigFile.WriteLine();
            
            rustConfigFile.Write($"#[cfg(all(target_pointer_width = \"{targetPointerWidth}\", ");
            if (!doublePrecision)
            {
                rustConfigFile.Write("not(");
            }
            rustConfigFile.Write("feature = \"double_precision\"");
            if (!doublePrecision)
            {
                rustConfigFile.Write(")");
            }
            rustConfigFile.WriteLine("))]");
            rustConfigFile.WriteLine("pub static VARIANT_SIZES: phf::Map<&'static str, usize> = phf_map! {");
            foreach (Api.ClassSize size in classSizes.Sizes.Select(id => id))
            {
                rustConfigFile.WriteLine($"\"{size.Name.VariantEnumType()}\" => {size.Size}usize,");
                BuiltinClassSizes[size.Name] = size.Size;
            }
            rustConfigFile.WriteLine("};");
        }
        rustConfigFile.Close();


        var abbreviations = new List<string> {
            "AABB",
            "RID",
        };

        // foreach ((string name, int index) in api.BuiltinClasses.OrderByDescending(c => c.Name.Length).Select((cls, i) => (Fixer.VariantName(cls.Name), i)))
        // {
        //     if (name.Contains("2d", StringComparison.InvariantCultureIgnoreCase)) Debugger.Break();
        //     Fixer.Words.Add(name);
        //     // bool isAbbreviation = abbreviations.Contains(name);
        //     Fixer.PascalCaseWords.Add(name, $"{{{index}}}");
        //     // string fixedName = isAbbreviation ? name : name.ToSnakeCase();
        //     // fixedName = fixedName.Replace("2_d", "2D");
        //     // fixedName = fixedName.Replace("3_d", "3D");
        //     // Fixer.SnakeCaseWords.Add(fixedName, $"{{{index}}}");
        // }
        
    }

    public Dictionary<string, List<string>> BuiltinClassFunctions { get; } = new();
    public Dictionary<string, List<string>> ClassFunctions { get; } = new();
    public Dictionary<string, List<string>> UtilityFunctions { get; } = new();
    public static readonly Dictionary<string, int> BuiltinClassSizes = new();

    public void Emit()
    {
        var rustGeneratedModFile = new StreamWriter(Path.Join(this.rsGeneratedDir, "mod.rs"));
        rustGeneratedModFile.WriteLine("pub mod config;");
        rustGeneratedModFile.WriteLine("pub mod builtin_classes;");
        // rustGeneratedModFile.WriteLine("pub mod classes;");
        rustGeneratedModFile.Close();
        foreach (string o in builtinObjectTypes)
        {
            objectTypes.Add(o);
        }

        foreach (Api.Class c in api.Classes)
        {
            objectTypes.Add(c.Name);
        }

        string builtinClassesCsDir = csDir + "/BuiltinClasses";
        string builtinClassesRustDir = rsGeneratedDir + "/builtin_classes";
        Directory.CreateDirectory(builtinClassesCsDir);
        Directory.CreateDirectory(builtinClassesRustDir);

        var variantCsFile = new StreamWriter(Path.Join(builtinClassesCsDir, "Variant.cs"));
        var variantCppHeaderFile = new StreamWriter(Path.Join(builtinClassesRustDir, "Variant.hpp"));
        var variantRustFile = new StreamWriter(Path.Join(builtinClassesRustDir, "Variant.rs"));
        WriteRustFileHeader(variantRustFile);

        variantCsFile.WriteLine("namespace GodotSharpGDExtension;");
        variantCsFile.WriteLine();
        variantCsFile.WriteLine("public partial class Variant {");           
        // TODO: Reactivate after classes are done
        BuiltinClasses(variantCsFile, variantCppHeaderFile, variantRustFile, builtinClassesCsDir, builtinClassesRustDir);
        return;
        Classes();

        Directory.CreateDirectory(csDir + "/Enums");
        // foreach (Api.Enum e in api.GlobalEnums)
        // {
        //     // TODO
        //     GlobalEnum(e, csDir + "/Enums");
        // }

        Directory.CreateDirectory(csDir + "/NativeStructures");
        // TODO?
        // foreach (Api.NativeStructure native in api.NativeStructures)
        // {
        //     StreamWriter file = File.CreateText(csDir + "/NativeStructures/" + Fixer.CSType(native.Name, api) + ".cs");
        //     file.WriteLine("namespace GDExtensionInterface;");
        //     file.WriteLine(value: "[StructLayout(LayoutKind.Sequential)]");
        //     file.WriteLine($"public unsafe struct {native.Name} {{");
        //     foreach (string member in native.Format.Split(";"))
        //     {
        //         string[] pair = member.Split(" ");
        //         string name = Fixer.Name(pair[1]);
        //         string type = Fixer.CSType(pair[0], api);
        //         if (name.Contains('*'))
        //         {
        //             type = "IntPtr"; //pointer to `'Object', which is managed in bindings
        //             name = name.Replace("*", "");
        //         }
        //         else if (builtinObjectTypes.Contains(type))
        //         {
        //             type = "IntPtr";
        //         }
        //         if (name.Contains('['))
        //         {
        //             int size = int.Parse(name.Split("[")[1].Split("]")[0]);
        //             name = name.Split("[")[0];
        //             for (var i = 0; i < size; i++)
        //             {
        //                 file.WriteLine($"\t{type} {name}{i};");
        //             }
        //             continue;
        //         }
        //         file.WriteLine($"\t{type} {name};");
        //     }
        //     file.WriteLine("}");
        //     file.Close();
        // }
        Directory.CreateDirectory(csDir + "/UtilityFunctions");
        Class? docGlobalScope = GetDocs("@GlobalScope");
        var files = new Dictionary<string, (StreamWriter csFile, StreamWriter cppHeaderFile, StreamWriter cppSourceFile)>();
        
        //TODO Reactivate after classes are done
        // foreach (Api.Method f in api.UtilityFunction)
        // {
        //     string category = string.Concat(f.Category![0].ToString().ToUpper(), f.Category.AsSpan(1));
        //     if (files.TryGetValue(category, out (StreamWriter csFile, StreamWriter cppHeaderFile, StreamWriter cppSourceFile) file) == false)
        //     {
        //         file = (
        //             File.CreateText($"{csDir}/UtilityFunctions/{category}.cs"), 
        //             File.CreateText($"{cppGeneratedDir}/UtilityFunctions/{category}.hpp"),
        //             File.CreateText($"{cppGeneratedDir}/UtilityFunctions/{category}.cpp"));
        //         files.Add(category, file);
        //         
        //         file.csFile.WriteLine("namespace GodotSharpGDExtension;");
        //         file.csFile.WriteLine($"public static unsafe partial class {category} {{");
        //         WriteCppFileHeaders(file.cppHeaderFile, file.cppSourceFile, category);
        //         file.cppHeaderFile.WriteLine("namespace GDExtensionInterface {");
        //     }
        //     Method? d = null;
        //     if (docGlobalScope is { methods: not null })
        //     {
        //         d = docGlobalScope.methods.FirstOrDefault(x => x.name == f.Name);
        //     }
        //
        //     if (!UtilityFunctions.ContainsKey(category))
        //     {
        //         UtilityFunctions[category] = new List<string>();
        //     }
        //
        //     List<string> functions = UtilityFunctions[category];
        //     
        //     Method(f, category, file.csFile, file.cppHeaderFile, file.cppSourceFile, MethodType.Utility, d, functions);
        // }

        foreach ((StreamWriter csFile, StreamWriter cppHeaderFile, StreamWriter cppSourceFile) in files.Values)
        {
            csFile.WriteLine("}");
            csFile.Close();
            cppHeaderFile.WriteLine("}");
            cppHeaderFile.Close();
            cppSourceFile.Close();
        }
        
        // TODO: Reactivate after classes are done
        // Variant(variantCsFile);
        
        variantCsFile.WriteLine("}");
        variantCppHeaderFile.WriteLine("}");
        variantCppHeaderFile.WriteLine("}");
        variantRustFile.WriteLine("}");
        variantCsFile.Close();
        variantCppHeaderFile.Close();
        variantRustFile.Close();
        
    }

    private Class? GetDocs(string? name)
    {
        if (docDir == null) { return null; }
        string path = docDir + name + ".xml";
        if (File.Exists(path))
        {
            FileStream file = File.OpenRead(path);
            var d = (Class)classXml.Deserialize(file)!;
            file.Close();
            return d;
        }

        return null;
    }

    private BuiltinClass? GetBuiltinDocs(string name)
    {
        if (docDir == null) { return null; }
        string path = docDir + name + ".xml";
        if (!File.Exists(path))
        {
            return null;
        }

        FileStream file = File.OpenRead(path);
        var d = (BuiltinClass)builtinXml.Deserialize(file)!;
        file.Close();
        return d;

    }

    private void GlobalEnum(Api.Enum e, string dir)
    {
        if (e.Name.Contains('.')) { return; }
        string name = Fixer.CSType(e.Name, api).csType.Replace(".", "");
        StreamWriter file = File.CreateText(dir + "/" + Fixer.CSType(name, api) + ".cs");
        file.WriteLine("namespace GodotSharpGDExtension {");
        Enum(e, file);
        file.WriteLine("}");
        file.Close();
    }

    private void BuiltinClasses(TextWriter variantCsFile, TextWriter variantCppHeaderFile,
        TextWriter variantCppSourceFile, string builtinClassesCsDir, string builtinClassesRustDir)
    {
 
        
        var generalVariantClassFunctions = new List<string>();
        BuiltinClassFunctions["Variant"] = generalVariantClassFunctions;

        StreamWriter classesFile = File.CreateText(builtinClassesRustDir + "/mod.rs");
        
        classesFile.WriteLine("""
                              //---------------------------------------------
                              // This file is generated. Changes will be lost
                              //---------------------------------------------

                              """);

        const string staticConstructorPattern = 
            """
            	public {0} 
                {{
                    return new Variant({1});
                }}
            """;
        
        foreach (Api.BuiltinClass c in api.BuiltinClasses)
        {
            switch (c.Name)
            {
                case "Nil": continue;
                case "int":
                case "float":
                case "bool":
                // case "string":
                    BuiltinClass? doc = GetBuiltinDocs(c.Name);
                    WriteVariantConversionFunctions(c, variantCsFile, variantCppHeaderFile, variantCppSourceFile,
                        BuiltinClassSizes[c.Name], Fixer.CSType(c.Name, api).csType, generalVariantClassFunctions, api);
                    WriteBuiltinConstructors(c, doc, variantCsFile, variantCppSourceFile, generalVariantClassFunctions,
                         () => new ValueTuple<string, string>("static Variant New", $"{c.Name.ToSnakeCaseWithGodotAbbreviations()}_variant_"), staticConstructorPattern, "Variant");
                    break;
                default:
                    BuiltinClass(c, builtinClassesCsDir, builtinClassesRustDir, builtinObjectTypes.Contains(c.Name), variantCsFile, variantCppHeaderFile, variantCppSourceFile, classesFile, generalVariantClassFunctions);
                        
                    break;
            }
        }
        classesFile.Close();
    }

    private void BuiltinClass(Api.BuiltinClass builtinClass, string builtinClassesCsDir, string builtinClassesCppDir, bool hasPointer, TextWriter variantCsFile, TextWriter variantCppHeaderFile, TextWriter variantCppSourceFile, TextWriter classesFile, ICollection<string> variantClassFunctions)
    {
        string className = builtinClass.Name;
        if (className == "Object") className = "GodotObject";
        (string csTypeName, bool partial) = Fixer.CSType(className, api);
        StreamWriter csFile = File.CreateText($"{builtinClassesCsDir}/{csTypeName}.cs");
        var rustModuleName = $"{csTypeName.ToSnakeCaseWithGodotAbbreviations()}";
        StreamWriter rustFile = File.CreateText($"{builtinClassesCppDir}/{rustModuleName}.rs");
        
        classesFile.WriteLine($"pub mod {rustModuleName};");
        WriteRustFileHeader(rustFile);

        // registrations["builtin"].Add(csTypeName);

        var classFunctions = new List<string>();
        BuiltinClassFunctions[className] = classFunctions;
        
        BuiltinClass? doc = GetBuiltinDocs(className);

        // WriteVariantConversionFunctions(builtinClass, variantCsFile, variantCppHeaderFile, variantCppSourceFile, size, csTypeName, variantClassFunctions, api);

        foreach (Api.BuiltinClassSizesOfConfig config in api.BuiltinClassSizes)
        {
            if (config.BuildConfiguration != configName)
            {
                continue;
            }

            foreach (Api.ClassSize sizePair in config.Sizes)
            {
                if (sizePair.Name != className)
                {
                    continue;
                }

                break;
            }
            break;
        }
        
        // TODO: remove?
        // if (hasPointer == false)
        // {
        //     csFile.WriteLine($"[StructLayout(LayoutKind.Explicit, Size = {size})]");
        // }
        rustFile.WriteLine("use crate::variant_constructor;");
        rustFile.WriteLine($"use godot::sys::{{__GdextUninitializedType, GDExtensionConstTypePtr, {builtinClass.Name.VariantEnumType()}}};");
        rustFile.WriteLine();
        
        csFile.WriteLine("namespace GodotSharpGDExtension;");
        csFile.WriteLine();
        csFile.WriteLine($"public unsafe {(partial ? "partial " : "")}class {csTypeName} : Variant<{csTypeName}> {{");    
        // TODO: remove?
        // csFile.WriteLine("\tstatic private bool _registered = false;");
        // csFile.WriteLine();

        // TODO: remove?
        // csFile.WriteLine($"\tpublic const int StructSize = {size};");
        csFile.WriteLine($"\tpublic {className}(__GdextType* ptr) : base(ptr) {{}}");
        csFile.WriteLine();

        if (builtinClass.IsKeyed)
        {
            //Dictionary
            //todo: manually as extension?
        }

        if (builtinClass.IndexingReturnType != null)
        {
            //array?
            //todo: manually as extension?
        }

        // TODO Variant Members
        // if (builtinClass.Members != null)
        // {
        //     foreach (Api.BuiltinMember member in builtinClass.Members)
        //     {
        //         Member? d = null;
        //         if (doc is { members: not null })
        //         {
        //             d = doc.members.FirstOrDefault(x => x.name == member.Name);
        //         }
        //         Member(member, csFile, cppHeaderFile, rustFile, className, d, classFunctions);
        //     }
        // }
        
        // TODO: Constants
        // if (c.Constants != null)
        // {
        //     foreach (Api.Constant con in c.Constants)
        //     {
        //         if (doc is { constants: not null })
        //         {
        //             Constant? d = doc.constants.FirstOrDefault(x => x.name == con.Name);
        //             if (d is { comment: not null })
        //             {
        //                 string com = Fixer.XMLComment(d.comment);
        //                 csFile.WriteLine(com);
        //             }
        //         }
        //         csFile.WriteLine($"\tpublic static {Fixer.CSType(con.Type, api)} {con.Name.ToLower().ToPascalCase()} => {Fixer.Value(con.Value)};");
        //     }
        //     csFile.WriteLine();
        // }

        WriteBuiltinConstructors(builtinClass, doc, csFile, rustFile, classFunctions);
        
        // TODO: Variant operators
        // if (builtinClass.Operators != null)
        // {
        //     foreach (Api.Operator op in builtinClass.Operators)
        //     {
        //         Operator? d = null;
        //         if (doc is { operators: not null })
        //         {
        //             d = doc.operators.FirstOrDefault(x => x.name == $"operator {op.Name}");
        //         }
        //         Operator(op, className, csFile, cppHeaderFile, rustFile, d, classFunctions);
        //     }
        // }
        
        // TODO: Variant enums
        // if (builtinClass.Enums != null)
        // {
        //     foreach (Api.Enum e in builtinClass.Enums)
        //     {
        //         Enum(e, csFile, doc?.constants);
        //     }
        // }

        // TODO: Variant methods
        // if (builtinClass.Methods != null)
        // {
        //     foreach (Api.Method meth in builtinClass.Methods)
        //     {
        //         Method? d = null;
        //         if (doc is { methods: not null })
        //         {
        //             d = doc.methods.FirstOrDefault(x => x.name == meth.Name);
        //         }
        //         Method(meth, className, csFile, cppHeaderFile, rustFile, MethodType.Native, d, classFunctions);
        //     }
        // }

        EqualAndHash(className, csFile);

        // TODO: Variant Destructor
        // (string cppTypeForDotnetInterop, string? destruction) = Fixer.GetDestructorDataForType(className);
        // var cppDestructorName = $"{className}_destructor";
        // var cppDestructorSignature = $"void {cppDestructorName}({cppTypeForDotnetInterop} p_self)";
        // rustFile.WriteLine($"{cppDestructorSignature} {{");
        // if (destruction is null)
        // {
        //     rustFile.WriteLine(
        //         $"\tstatic auto destructor_func = godot::internal::gdextension_interface_variant_get_ptr_destructor({builtinClass.VariantName});");
        //     rustFile.WriteLine("\tdestructor_func(p_self.pointer);");
        // }
        // else
        // {
        //     rustFile.WriteLine($"\t{string.Format(destruction, "p_self")};");
        // }
        // rustFile.WriteLine("}");
        // csFile.WriteLine($"\t~{csTypeName}() {{");
        // csFile.WriteLine($"\t\tGDExtensionInterface.{builtinClass.Name}Destructor(this);");
        // csFile.WriteLine("\t}");
        //
        // classFunctions.Add(cppDestructorName);
        
        // if (hasPointer)
        // {
        //     csFile.WriteLine($"\t~{csTypeName}() {{");
        //     //file.WriteLine($"\t\tif(internalPointer == null) {{ return; }}");
        //     csFile.WriteLine($"\t\tGDExtensionInterface.CallGDExtensionPtrDestructor(__destructor, internalPointer);");
        //     //file.WriteLine($"\t\tGDExtensionInterface.MemFree(internalPointer);");
        //     //file.WriteLine($"\t\tinternalPointer = null;");
        //     csFile.WriteLine($"\t}}");
        //     csFile.WriteLine();
        //     csFile.WriteLine("\t[StructLayout(LayoutKind.Explicit, Size = StructSize)]");
        //     csFile.WriteLine("\tpublic struct InternalStruct { }");
        //     csFile.WriteLine();
        // }


        // for (var i = 0; i < constructorRegistrations.Count; i++)
        // {
        //     csFile.WriteLine($"\tstatic IntPtr __constructorPointer{i} => {constructorRegistrations[i]};");
        // }
        // for (var i = 0; i < operatorRegistrations.Count; i++)
        // {
        //     csFile.WriteLine($"\tstatic IntPtr __operatorPointer{i} => {operatorRegistrations[i]};");
        // }
        // for (var i = 0; i < methodRegistrations.Count; i++)
        // {
        //     csFile.WriteLine($"\tstatic IntPtr __methodPointer{i} => {methodRegistrations[i]};");
        // }


        csFile.WriteLine();
        csFile.WriteLine("}");
        csFile.Close();
        rustFile.Close();
    }

    private static void WriteVariantConversionFunctions(Api.BuiltinClass builtinClass, TextWriter variantCsFile,
        TextWriter variantCppHeaderFile, TextWriter variantCppSourceFile, int size,
        string csTypeName, ICollection<string> variantClassFunctions, Api api)
    {
        var nativeToFunctionName = $"variant_to_{builtinClass.Name}";
        variantClassFunctions.Add(nativeToFunctionName);

        string godotCppType = Fixer.GodotCppType(builtinClass.Name, api);
        string returnConstruct = Fixer.GetConstructionForGodotType(builtinClass.Name);
        (string typeForDotnetInterop, string conversion, string _, bool canDotnetBePassedByValue, bool canGodotBePassedByValue) = Fixer.GetConvertToDotnetDataForType(godotCppType);
        
        var cppToFunctionSignature = $"{typeForDotnetInterop} {{0}}{nativeToFunctionName}(GodotType variant)";
        variantCppHeaderFile.WriteLine($"GDE_EXPORT {string.Format(cppToFunctionSignature, "")};");
        variantCppSourceFile.WriteLine($"{string.Format(cppToFunctionSignature, "GDExtensionInterface::")} {{");
        variantCppSourceFile.WriteLine(
            $"\tconst auto func = godot::internal::gdextension_interface_get_variant_to_type_constructor({builtinClass.VariantName});");
        variantCppSourceFile.Write("\t");
        variantCppSourceFile.WriteLine(returnConstruct, builtinClass.Name.ToSnakeCaseWithGodotAbbreviations(), "new_instance");
        variantCppSourceFile.WriteLine($"\tfunc({(canGodotBePassedByValue ? "&new_instance" : "new_instance.pointer")}, variant.pointer);");
        variantCppSourceFile.WriteLine($"\treturn {conversion};", "new_instance");
        variantCppSourceFile.WriteLine("}");
        variantCsFile.WriteLine($"\tpublic {csTypeName} As{Fixer.VariantName(csTypeName)}()");
        variantCsFile.WriteLine("\t{");
        variantCsFile.WriteLine(
            $"\t\treturn {(canDotnetBePassedByValue ? "" : $"new {csTypeName}(") }GDExtensionInterface.{nativeToFunctionName.ToPascalCase()}(this){(canDotnetBePassedByValue ? "" : ".Pointer)")};");
        variantCsFile.WriteLine("\t}");
    }

    private void WriteBuiltinConstructors(Api.BuiltinClass c, BuiltinClass? doc, TextWriter csFile, TextWriter rustFile, ICollection<string> classFunctions, Func<(string csConstructorNamePrefix, string cppConstructorNamePrefix)>? getConstructorNamePrefixes = null, string csConstructorPattern = DefaultBuiltinCsConstructorPattern, string? generatedCsClassName = null)
    {
        (string csConstructorNamePattern, string cppConstructorNamePrefix)? prefixResult = getConstructorNamePrefixes?.Invoke();
        
        if (c.Constructors != null)
        {
            var maxConstructorIndex = 0;
            for (var i = 0; i < c.Constructors.Length; i++)
            {
                Api.Constructor constructor = c.Constructors[i];
                Constructor? d = null;
                if (doc is { constructors: not null })
                {
                    d = doc.constructors[i];
                }

                maxConstructorIndex = Math.Max(maxConstructorIndex, constructor.Index);
                Constructor(c, constructor, csFile, rustFile, d, classFunctions, getConstructorNamePrefixes, csConstructorPattern, generatedCsClassName);
            }
        }
        else
        {
            var emptyApiConstructor = new Api.Constructor { Arguments = Array.Empty<Api.Argument>(), Index = 0 };
            var emptyDocConstructor = new Constructor();
            Constructor(c, emptyApiConstructor, csFile, rustFile, emptyDocConstructor,
                classFunctions, getConstructorNamePrefixes, csConstructorPattern, generatedCsClassName);
        }
        rustFile.WriteLine($"const {c.Name.ToScreamingCaseSnakeWithGodotAbbreviations()}_DEFAULT_CONSTRUCTOR : unsafe extern \"C\" fn() -> GDExtensionConstTypePtr = {prefixResult?.cppConstructorNamePrefix ?? $"{c.Name.ToSnakeCaseWithGodotAbbreviations()}_"}constructor_0;");
    }

    private static void WriteRustFileHeader(TextWriter rustFile)
    {
        rustFile.WriteLine("""
                           //---------------------------------------------
                           // This file is generated. Changes will be lost
                           //---------------------------------------------
                           """);
    }

    private void Member(Api.BuiltinMember builtinMember, TextWriter csFile,
        TextWriter cppHeaderFile, TextWriter cppSourceFile, string className, Member? doc, ICollection<string> classFunctions)
    {
        if (doc != null)
        {
            string com = Fixer.XMLComment(doc.comment);
            csFile.WriteLine(com);
        }

        var getterName = $"{className}_{builtinMember.Name}_getter";
        var setterName = $"{className}_{builtinMember.Name}_setter";

        string cppType = Fixer.GodotCppType(builtinMember.Type, api);
        string csType = Fixer.CSType(builtinMember.Type, api).csType;
        string returnConstruct = Fixer.GetConstructionForGodotType(builtinMember.Type);
        (string getterMemberType, string getterConversion, string _, bool canGetterDotnetTypeBePassedByValue, bool canGetterGodotTypeBePassedByValue) = Fixer.GetConvertToDotnetDataForType(cppType);
        (string setterMemberType, string setterConversion, bool canSetterDotnetTypeBePassedByValue, bool canSetterGodotTypeBePassedByValue) = Fixer.GetConvertFromDotnetDataForType(cppType);
        
        var getterSignature = $"{getterMemberType} {{0}}{getterName}(GodotType p_self)";
        var setterSignature = $"void {{0}}{setterName}(GodotType p_self, {setterMemberType} p_value)";
        var getterCall = $"GDExtensionInterface.{getterName.ToPascalCaseWithGodotAbbreviations()}(this)";
        csFile.WriteLine($$"""
                           	public {{csType}} {{builtinMember.Name.ToPascalCase()}}
                           	{
                           		get => {{(canGetterDotnetTypeBePassedByValue ? getterCall : $"new({getterCall}.Pointer)")}};
                           		set => GDExtensionInterface.{{setterName.ToPascalCaseWithGodotAbbreviations()}}(this, {{(canSetterDotnetTypeBePassedByValue ? "value" : "value")}});
                           	}
                           """);
        cppHeaderFile.WriteLine($"""
                                 GDE_EXPORT {string.Format(getterSignature, "")};
                                 GDE_EXPORT {string.Format(setterSignature, "")};
                                 """);

        cppSourceFile.WriteLine();
        cppSourceFile.WriteLine($"{string.Format(getterSignature, "GDExtensionInterface::")} {{");
        string variantEnumType = className.VariantEnumType();
        cppSourceFile.WriteLine($"\tstatic auto getter = godot::internal::gdextension_interface_variant_get_ptr_getter({variantEnumType}, godot::StringName(\"{builtinMember.Name}\")._native_ptr());");
        cppSourceFile.Write("\t");
        cppSourceFile.WriteLine(returnConstruct, builtinMember.Type.ToSnakeCaseWithGodotAbbreviations(), "value");
        cppSourceFile.Write(
            canGetterGodotTypeBePassedByValue
            ? "\tgetter(p_self.pointer, &value);"
            : "\tgetter(p_self.pointer, value.pointer);");
        cppSourceFile.WriteLine($"\treturn {getterConversion};", "value");
        cppSourceFile.WriteLine("}");

        cppSourceFile.WriteLine($"{string.Format(setterSignature, "GDExtensionInterface::")} {{");
        cppSourceFile.WriteLine($"\tstatic auto setter = godot::internal::gdextension_interface_variant_get_ptr_setter({variantEnumType}, godot::StringName(\"{builtinMember.Name}\")._native_ptr());");
        cppSourceFile.WriteLine($"\tauto value = {string.Format(setterConversion, "p_value")};");
        cppSourceFile.WriteLine(
            canSetterGodotTypeBePassedByValue 
                ? "\tsetter(p_self.pointer, &value);"
                : "\tsetter(p_self.pointer, value);");
        cppSourceFile.WriteLine("}");        
        classFunctions.Add(getterName);
        classFunctions.Add(setterName);
        csFile.WriteLine();
    }

    private void Constructor(Api.BuiltinClass c, Api.Constructor constructor, TextWriter csFile,
        TextWriter rustFile, Constructor? doc, ICollection<string> classFunctions, Func<(string csConstructorNamePrefix, string cppConstructorNamePrefix)>? getConstructorNamePrefixes, string csConstructorPattern, string? generatedCsClassName)
    {
        (string csConstructorNamePattern, string cppConstructorNamePrefix)? prefixResult = getConstructorNamePrefixes?.Invoke();

        if (doc != null)
        {
            string com = Fixer.XMLComment(doc.description);
            csFile.WriteLine(com);
        }
        var csArgs = new List<string>();
        var nativeArgs = new List<string>();
        var nativeArgNames = new List<string>();
        var csArgPasses = new List<string>();
        // var nativeArgPasses = new List<string>();
        // var nativeArgConversions = new List<string>();
        
        if (constructor.Arguments != null)
        {
            foreach (Api.Argument arg in constructor.Arguments)
            {
                string rustType = Fixer.GodotCppType(arg.Type, api);
                (string type, string? conversion, bool canInputBePassedByValue, bool canConvertedBePassedByValue) = Fixer.GetConvertFromDotnetDataForType(rustType);

                string csArgName = Fixer.Name(arg.Name).ToCamelCase();
                csArgs.Add($"{Fixer.CSType(arg.Type, api).csType} {csArgName}");
                var cppArg = $"p_{arg.Name}";
                nativeArgNames.Add(cppArg);
                nativeArgs.Add($"{cppArg} : GDExtensionConstTypePtr");
                csArgPasses.Add(canInputBePassedByValue ? $"(__GdextType*)(&{csArgName})" : csArgName);
                // nativeArgConversions.Add($"auto {arg.Name} = {string.Format(conversion, cppArg)}");
                // nativeArgPasses.Add(canConvertedBePassedByValue ? $"&{arg.Name}" : $"{arg.Name}");
            }
        }
        const string argSeparator = ", ";

        var nativeFunctionName = $"{prefixResult?.cppConstructorNamePrefix ?? $"{c.Name.ToSnakeCaseWithGodotAbbreviations()}_"}constructor_{constructor.Index}";
        
        var rustFunctionSignature = $"{nativeFunctionName}({string.Join(argSeparator, nativeArgs)})";
        
        // static auto constructor = godot::internal::gdextension_interface_variant_get_ptr_constructor(GDEXTENSION_VARIANT_TYPE_AABB, 1);
        // auto base = new uint8_t[8];
        // std::array<GDExtensionConstTypePtr, 1> call_args = {from};
        // constructor(base, call_args.data());
        // return base;

        rustFile.WriteLine("#[no_mangle]");
        rustFile.WriteLine($"pub unsafe extern \"C\" fn {rustFunctionSignature} -> GDExtensionConstTypePtr {{");
        rustFile.Write($"\tvariant_constructor!({c.Name.VariantEnumType()}, {constructor.Index}");
        if (nativeArgs.Any())
        {
            rustFile.Write($", {string.Join(", ", nativeArgNames)}");            
        }
        rustFile.WriteLine(")");
        rustFile.WriteLine("}");
        rustFile.WriteLine();
        
        
        // csFile.Write($"\tpublic {prefixResult?.csConstructorNamePrefix}{Fixer.CSType(c.Name, api).ToPascalCase()}(");
        // csFile.Write(string.Join(argSeparator, csArgs));
        // csFile.WriteLine(") {");

        var csFunctionSignature = $"{prefixResult?.csConstructorNamePattern}{Fixer.VariantName(c.Name)}({string.Join(argSeparator, csArgs)})";

        var csCallText = $"NativeMethods.{nativeFunctionName}({string.Join(argSeparator, csArgPasses)})";
        // csFile.Write("\t\t");
        csFile.WriteLine(csConstructorPattern, csFunctionSignature, csCallText);
        // csFile.WriteLine("\t}");
        
        
        // TODO: remove?
        // if (hasPointer == false)
        // {
        //     csFile.WriteLine($"\t\tfixed ({Fixer.Type(c.name, api)}* ptr = &this) {{");
        //     csFile.Write("\t\t\tGDExtensionInterface.CallGDExtensionPtrConstructor(constructor, (IntPtr)ptr, ");
        // }
        // else
        // {
        //     csFile.Write("\t\tGDExtensionInterface.CallGDExtensionPtrConstructor(constructor, internalPointer, ");
        // }
        // if (constructor.arguments != null)
        // {
        //     csFile.WriteLine("*args);");
        // }
        // else
        // {
        //     csFile.WriteLine("IntPtr.Zero);");
        // }
        // if (hasPointer == false)
        // {
        //     csFile.WriteLine("\t\t}");
        // }
        // csFile.WriteLine("\t}");
        csFile.WriteLine();
        classFunctions.Add(nativeFunctionName);
    }

    private void Operator(Api.Operator op, string className, TextWriter csFile, TextWriter cppHeaderFile, TextWriter cppSourceFile, Operator? doc, ICollection<string> classFunctions)
    {
        string godotCppLeftType = Fixer.GodotCppType(className, api);
        string godotCppReturnType = Fixer.GodotCppType(op.ReturnType, api);
        
        string returnConstruct = Fixer.GetConstructionForGodotType(op.ReturnType);
        (string returnTypeToDotnet, string returnConversion, string _, bool canDotnetBePassedByValue, bool canGodotBePassedByValue) = Fixer.GetConvertToDotnetDataForType(godotCppReturnType);

        (string leftTypeFromDotnet, string leftConversion, bool canLeftInputTypeBePassedByValue, bool canLeftConvertedTypeBePassedByValue) = Fixer.GetConvertFromDotnetDataForType(godotCppLeftType);

        string csReturnType = Fixer.CSType(op.ReturnType, api).csType;
        string variantEnumType = className.VariantEnumType();
        string? rightClassName = op.RightType;
        if (rightClassName != null)
        {
            if (rightClassName == "Variant") { return; }
            
            string godotCppRightType = Fixer.GodotCppType(rightClassName, api);
            (string rightTypeFromDotnet, string rightConversion, bool canRightInputTypeBePassedByValue, bool canRightConvertedTypeBePassedByValue) = Fixer.GetConvertFromDotnetDataForType(godotCppRightType);

            var cppOperatorName = $"{op.Name.VariantOperatorCpp()}";

            var nativeFunctionName = $"{className}_{cppOperatorName}_{rightClassName}_operator";
            
            string name = op.Name switch
            {
                "or" => "operator |",
                "and" => "operator &",
                "xor" => "operator ^",
                "**" => "operator_power",
                "in" => "operator_in",
                _ => $"operator {op.Name}",
            };
            if (doc != null)
            {
                csFile.WriteLine(Fixer.XMLComment(doc.description));
            }


            
            var cppFunctionSignature = $"{returnTypeToDotnet} {{0}}{nativeFunctionName}({leftTypeFromDotnet} p_left, {rightTypeFromDotnet} p_right)";
            cppHeaderFile.WriteLine($"GDE_EXPORT {string.Format(cppFunctionSignature, "")};");
            
            cppSourceFile.WriteLine($"{string.Format(cppFunctionSignature, "GDExtensionInterface::")} {{");
            cppSourceFile.WriteLine($"\tauto left = {string.Format(leftConversion, "p_left")};");
            cppSourceFile.WriteLine($"\tauto right = {string.Format(rightConversion, "p_right")};");
            cppSourceFile.WriteLine($"\tstatic auto operator_func = godot::internal::gdextension_interface_variant_get_ptr_operator_evaluator({op.Name.VariantOperatorEnum()}, {variantEnumType}, {rightClassName.VariantEnumType()});");
            cppSourceFile.Write("\t");
            cppSourceFile.WriteLine(returnConstruct, op.ReturnType.ToSnakeCaseWithGodotAbbreviations(), "ret");
            cppSourceFile.WriteLine($"\toperator_func({(canLeftConvertedTypeBePassedByValue ? "&left" : "left")}, {(canRightConvertedTypeBePassedByValue ? "&right" : "right")}, {(canDotnetBePassedByValue ? "&ret" : "ret.pointer")});");
            
            classFunctions.Add(nativeFunctionName);
            
            csFile.WriteLine($"\tpublic static {csReturnType} {name}({Fixer.CSType(className, api).csType} left, {Fixer.CSType(rightClassName, api).csType} right) {{");
            csFile.WriteLine($"\t\t{(canDotnetBePassedByValue ? csReturnType : "GodotType")} result = GDExtensionInterface.{nativeFunctionName.ToPascalCaseWithGodotAbbreviations()}({(canLeftInputTypeBePassedByValue ? "left" : "left")}, {(canRightInputTypeBePassedByValue ? "right" : "right")});");
            csFile.WriteLine($"\t\treturn {(canDotnetBePassedByValue ? "result" : $"new {csReturnType}(result.Pointer)")};");            
            
        }
        else
        {
            var cppOperatorName = $"operator_{op.Name.VariantOperatorCpp()}";

            var nativeFunctionName = $"{className}_{cppOperatorName}";
            
            string name = op.Name switch
            {
                "unary-" => "operator -",
                "not" => "operator !",
                "unary+" => "operator +",
                _ => $"operator {op.Name}",
            };
            if (doc != null)
            {
                csFile.WriteLine(Fixer.XMLComment(doc.description));
            }
            
            var cppFunctionSignature = $"{returnTypeToDotnet} {{0}}{nativeFunctionName}({leftTypeFromDotnet} p_left)";
            cppHeaderFile.WriteLine($"GDE_EXPORT {string.Format(cppFunctionSignature, "")};");
            
            cppSourceFile.WriteLine($"{string.Format(cppFunctionSignature, "GDExtensionInterface::")} {{");
            cppSourceFile.WriteLine($"\tstatic auto operator_func = godot::internal::gdextension_interface_variant_get_ptr_operator_evaluator({op.Name.VariantOperatorEnum()}, {variantEnumType}, {"Nil".VariantEnumType()});");
            cppSourceFile.Write("\t");
            cppSourceFile.WriteLine(returnConstruct, op.ReturnType.ToSnakeCaseWithGodotAbbreviations(), "ret");
            cppSourceFile.WriteLine($"\tauto left = {string.Format(leftConversion, "p_left")};");
            cppSourceFile.WriteLine($"\toperator_func({(canLeftConvertedTypeBePassedByValue ? "&left" : "left")}, nullptr, {(canDotnetBePassedByValue ? "&ret" : "ret.pointer")});");
            
            classFunctions.Add(nativeFunctionName);
            
            csFile.WriteLine($"\tpublic static {csReturnType} {name}({Fixer.CSType(className, api).csType} left) {{");
            csFile.WriteLine($"\t\t{(canDotnetBePassedByValue ? csReturnType : "GodotType")} result = GDExtensionInterface.{nativeFunctionName.ToPascalCaseWithGodotAbbreviations()}({(canLeftInputTypeBePassedByValue ? "left" : "left")});");
            csFile.WriteLine($"\t\treturn {(canDotnetBePassedByValue ? "result" : $"new {csReturnType}(result.Pointer)")};");
        }
        cppSourceFile.WriteLine($"\treturn {returnConversion};", "ret");
        cppSourceFile.WriteLine("}");
        csFile.WriteLine("\t}");
        csFile.WriteLine();
    }

    private void Enum(Api.Enum e, StreamWriter file, Constant[]? constants = null)
    {
        int prefixLength = Fixer.SharedPrefixLength(e.Values.Select(x => x.Name).ToArray());
        if (e.IsBitfield ?? false)
        {
            file.WriteLine("\t[Flags]");
        }

        file.WriteLine($"\tpublic enum {Fixer.CSType(e.Name, api).csType} {{");
        foreach (Api.ValueData v in e.Values)
        {
            Constant? d = constants?.FirstOrDefault(x => x.@enum != null && x.@enum == e.Name && x.name == v.Name);
            if (d is { comment: not null })
            {
                file.WriteLine(Fixer.XMLComment(d.comment, 2));
            }
            string name = v.Name[prefixLength..].ToPascalCase();
            if (char.IsDigit(name[0])) { name = "_" + name; }
            file.WriteLine($"\t\t{name} = {v.Value},");
        }
        file.WriteLine("\t}");
        file.WriteLine();
    }

    private string ValueToPointer(string name, string type)
    {
        string f = Fixer.CSType(type, api).csType;
        if (type == "String")
        {
            return $"StringMarshall.ToNative({name})";
        }

        if (objectTypes.Contains(type) || builtinObjectTypes.Contains(f))
        {
            return $"{name}";
        }
        return $"(IntPtr)(&{Fixer.Name(name)})";
    }

    private string ReturnStatementValue(string type)
    {
        string f = Fixer.CSType(type, api).csType;
        if (type == "String")
        {
            return "StringMarshall.ToManaged(__res)";
        }

        if (builtinObjectTypes.Contains(f))
        {
            return f == "Array" 
                ? $"new {f}(GDExtensionMain.MoveToUnmanaged(__res))" 
                : $"new {f}(__res)";
        }
        switch (f)
        {
            case "Variant":
                return "__res";
            case "GodotObject":
                return "GodotObject.ConstructUnknown(__res)";
        }

        if (objectTypes.Contains(type))
        {
            return $"({f})GodotObject.ConstructUnknown(__res)";
        }
        string marshalText;
        string castText;
        int lastIndex = type.LastIndexOf("::", StringComparison.Ordinal);
        var enumTypes = new List<string>
        {
            "enum",
            "bitfield",
        };
            
        if (lastIndex != -1 && enumTypes.Contains(type[..(lastIndex)]))
        {
            castText = $"({type[(lastIndex + 2)..]})";
            marshalText = "Marshal.ReadInt32(__res)";
        }
        else if (lastIndex != -1 && type[..lastIndex] == "typedarray")
        {
            castText = $"new {f}(__res)";
            marshalText = "";
        }
        else
        {
            switch (type)
            {
                case "int":
                {
                    castText = "";
                    marshalText = "Marshal.ReadInt32(__res)";
                    break;
                }
                case "long":
                {
                    castText = "";
                    marshalText = "Marshal.ReadInt64(__res)";
                    break;
                }
                case "bool":
                {
                    castText = "";
                    marshalText = "Marshal.ReadByte(__res) != 0";
                    break;
                }
                default:
                {
                    castText = "";
                    marshalText = $"Marshal.PtrToStructure<{type}>(__res)";
                    break;
                }
            }
        }
            
        return $"{castText}{marshalText}";
    }

    private enum MethodType
    {
        Class,
        Native,
        Utility,
    }

    private static bool IsValidDefaultValue(string value, string type)
    {
        if (value.Contains('(')) { return false; }
        if (value.Contains('&')) { return false; }
        switch (value)
        {
            case "{}":
            case "[]":
                return false;
            case "":
                return type == "String";
        }

        switch (type)
        {
            case "Variant":
            case "StringName":
                return false;
            default:
                return true;
        }
    }

    private string FixDefaultValue(string value, string type)
    {
        if (value.StartsWith("Array["))
        {
            return "new()";
        }
        if (value.Contains('(')) { return $"new {value}"; }
        switch (value)
        {
            case "{}":
                return "new Dictionary()";
            case "[]":
                return "new()";
            case "":
                return $"new {type}()";
            case "null":
                return "null";
        }

        if (value.Contains('&')) { return $"new StringName({value[1..]})"; }
        if (type == "Variant" && value == "null") { return "Variant.Nil"; }
        return $"({Fixer.CSType(type, api).csType}){value}";
    }

    private void Method(Api.Method meth, string classOrCategory, TextWriter csFile, TextWriter cppHeaderFile, TextWriter cppSourceFile, MethodType type, Method? doc, ICollection<string> functions, bool isSingleton = false)
    {
        string cppFunctionSignature;
        
        var csFunctionSignature = "";
        if (doc is { description: not null })
        {
            csFunctionSignature += Fixer.XMLComment(doc.description) + Environment.NewLine;
        }
        csFunctionSignature += "\tpublic ";
        string ret = meth.ReturnType ?? meth.ReturnValue?.Type ?? "";
        bool isStaticMethod = (meth.IsStatic ?? false) || type == MethodType.Utility || isSingleton;
        if (isStaticMethod)
        {
            csFunctionSignature += "static ";
        }
        if (meth.IsVirtual)
        {
            csFunctionSignature += "virtual ";
        }
        if (meth.Name == "to_string")
        {
            csFunctionSignature += "new ";
        }


        var csArguments = new List<string>();
        var cppArguments = new List<string>();
        var cppCallArgs = new List<string>();
        var argConversions = new Dictionary<string, (string conversion, bool canBePassedByValue)>();
        var csArgumentNames = new List<string>();
        var cppArgumentNames = new List<string>();
        var csNativeCallArgs = new List<string>();
        
        if (!isStaticMethod)
        {
            cppArguments.Add("const GodotType p_self");
            csNativeCallArgs.Add("this");
        }

        // if (type != MethodType.Utility)
        // {
        //     if (!isStaticMethod)
        //     {
        //         csNativeCallArgs.Add("this");
        //         // csFile.Write("this");
        //         // if (argumentCount > 0)
        //         // {
        //         //     csFile.Write(", ");
        //         // }
        //     }
        // }        
        
        int argumentCount = meth.Arguments?.Length ?? 0;
        if (argumentCount > 0)
        {
            for (var i = 0; i < argumentCount; i++)
            {
                Api.Argument arg = meth.Arguments[i];
                var suffix = "";
                string argumentName = Fixer.CppArgumentName(meth.Arguments[i].Name);
                
                // cppArgumentNames.Add($"p_{argumentName}");
                
                string godotCppType = Fixer.GodotCppType(meth.Arguments[i].Type, api);
                (string cppArgTypeForDotnetInterop, string argConversion, bool canArgBePassedByValue, bool canConvertedArgBePassedByValue) = Fixer.GetConvertFromDotnetDataForType(godotCppType);
                
                argConversions.Add(argumentName, new ValueTuple<string, bool>(argConversion, canConvertedArgBePassedByValue));
                cppCallArgs.Add(argumentName);
                cppArguments.Add($"{cppArgTypeForDotnetInterop} p_{argumentName}");
                var validDefault = false;

                string csArgumentName = Fixer.CSArgumentName(argumentName);
                if (arg.DefaultValue != null)
                {
                    for (int j = i; j < argumentCount; j++)
                    {
                        validDefault &= IsValidDefaultValue(meth.Arguments[j].DefaultValue!, meth.Arguments[j].Type);
                    }

                    if (validDefault)
                    {
                        csArguments.Add($"{Fixer.CSType(arg.Type, api).csType} {csArgumentName} = {FixDefaultValue(arg.DefaultValue, arg.Type)}");
                    }
                    else
                    {
                        
                        // csFile.Write(header + $") => {Fixer.MethodName(meth.Name)}(");
                        // for (var j = 0; j < i; j++)
                        // {
                        //     csFile.Write($"{Fixer.Name(meth.Arguments[j].Name)}, ");
                        // }
                        // for (int j = i; j < meth.Arguments.Length; j++)
                        // {
                        //     csFile.Write($"{FixDefaultValue(meth.Arguments[j].DefaultValue!, meth.Arguments[j].Type)}");
                        //     if (j < meth.Arguments.Length - 1)
                        //     {
                        //         csFile.Write(", ");
                        //     }
                        // }
                        // csFile.WriteLine(");");
                    }
                }

                if (!validDefault)
                {
                    csArguments.Add($"{Fixer.CSType(arg.Type, api).csType} {csArgumentName}");                    
                }
                
                // if (i != 0)
                // {
                //     header += ", ";
                // }
                // header += $"{Fixer.CSType(arg.Type, api).csType} {Fixer.Name(arg.Name)}{suffix}";
                csNativeCallArgs.Add(csArgumentName);
            }
        }
        if (meth.IsVararg)
        {
            csArguments.Add("params GodotType[] parameters");
            csArgumentNames.Add("parameters");
            
            cppArguments.Add("GodotType p_varargs[]");
            cppCallArgs.Add("varargs");
            cppArguments.Add("int p_vararg_count");
            cppArgumentNames.Add("varargs");
            cppArgumentNames.Add("vararg_count");
            // string t;
            // if (type == MethodType.Class)
            // {
            //     t = "IntPtr";
            // }
            // else
            // {
            //     t = "IntPtr";
            // }
            // if (meth.Arguments != null)
            // {
            //     csFile.WriteLine($"\t\tvar __args = stackalloc {t}[{meth.Arguments.Length} + arguments.Length];");
            // }
            // else
            // {
            //     csFile.WriteLine($"\t\tvar __args = stackalloc {t}[arguments.Length];");
            // }
        }

        string csType = Fixer.CSType(ret, api).csType;
        bool hasReturnValue = !string.IsNullOrEmpty(ret);
        var conversion = "";
        var canGodotBePassedByValue = false;
        var csCallPattern = "{0};";
        string returnConstruct = Fixer.GetConstructionForGodotType(ret);
        (string? cppTypeForDotnetInterop, conversion, string _, bool canDotnetBePassedByValue, canGodotBePassedByValue) = Fixer.GetConvertToDotnetDataForType(Fixer.GodotCppType(ret, api));
        if (hasReturnValue)
        {
            csFunctionSignature += csType;
            cppFunctionSignature = cppTypeForDotnetInterop;
        }
        else
        {
            csFunctionSignature += "void";
            cppFunctionSignature = "void";
        }

        string cppMethodName = Fixer.MethodName(meth.Name);
        string csMethodName = cppMethodName.ToPascalCaseWithGodotAbbreviations();
        string cppFunctionName = string.IsNullOrWhiteSpace(classOrCategory) ? cppMethodName : $"{classOrCategory.ToSnakeCaseWithGodotAbbreviations()}_{cppMethodName}";
        functions.Add(cppFunctionName);
        cppFunctionSignature += $" {{0}}{cppFunctionName}({string.Join(", ", cppArguments)})";
        csFunctionSignature += $" {csMethodName}({string.Join(", ", csArguments)})";
        
        cppHeaderFile.WriteLine($"GDE_EXPORT {string.Format(cppFunctionSignature, "")};");
        cppSourceFile.WriteLine($"{string.Format(cppFunctionSignature, "GDExtensionInterface::")} {{");

        cppSourceFile.WriteLine($"\tconst godot::StringName METHOD_NAME = \"{meth.Name}\";");
        
        cppSourceFile.Write("\tstatic const auto METHOD = godot::internal::");

        // gdextension_interface_classdb_get_method_bind
        string getMethod = type switch
        {
            MethodType.Utility => "gdextension_interface_variant_get_ptr_utility_function(",
            MethodType.Native => $"gdextension_interface_variant_get_ptr_builtin_method({classOrCategory.VariantEnumType()}, ",
            MethodType.Class => "gdextension_interface_classdb_get_method_bind(class_name._native_ptr(), ",
        };
        cppSourceFile.Write(getMethod);
        
        cppSourceFile.WriteLine($"METHOD_NAME._native_ptr(), {meth.Hash});");
        
        csFile.WriteLine($"{csFunctionSignature} {{");

        if (meth.IsVararg)
        {
            csNativeCallArgs.Add("parameters");
        }

        csFile.Write("\t\t");
        // if (hasReturnValue)
        // {
        //     csFile.Write("var ret = ");
        // }


        if (meth.Arguments != null || meth.IsVararg)
        {
            //std::array<GodotType, 2> args = {to, &weight};
            //cppCallArgs

            foreach ((string argumentName, (string argConversion, bool _)) in argConversions)
            {
                cppSourceFile.Write($"\tauto {argumentName} = ");
                cppSourceFile.Write(argConversion, $"p_{argumentName}");
                cppSourceFile.WriteLine(";");
            }
            
            cppSourceFile.WriteLine($"\tauto args = new GDExtensionTypePtr[{argumentCount}{(meth.IsVararg ? " + p_vararg_count" : "")}];");
            var index = 0;
            foreach (string callArg in cppCallArgs)
            {
                string argConversion;
                if (argConversions.TryGetValue(callArg, out (string conversion, bool canBePassedByValue) argData))
                {
                    // (string argConversion, bool canBePassedByValue) = ;
                    argConversion = $"{(argData.canBePassedByValue ? "&" : "")}{callArg}";
                }
                else
                {
                    argConversion = $"p_{callArg}";
                }
                
                cppSourceFile.WriteLine($"\targs[{index++}] = {argConversion};");
            }
            // cppSourceFile.WriteLine($"std::array<GodotType, {cppArguments.Count}> args = {{to, &weight}};");
        }

        if (hasReturnValue)
        {

            csCallPattern = canDotnetBePassedByValue
                ? "return {0};"
                : $"return new {csType}({{0}}.Pointer);";
            
            cppSourceFile.Write("\t");
            cppSourceFile.WriteLine(returnConstruct, ret.ToSnakeCaseWithGodotAbbreviations(), "ret");
        }

        switch (type)
        {
            case MethodType.Class:
                cppSourceFile.Write("\tgodot::internal::gdextension_interface_object_method_bind_ptrcall(METHOD, ");
                break;
            case MethodType.Native:
            case MethodType.Utility:
                cppSourceFile.Write("\tMETHOD(");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
        
        if (type != MethodType.Utility)
        {
            cppSourceFile.Write(!isStaticMethod ? "p_self.pointer, " : "nullptr, ");
            WriteArgsToCall();
            WriteReturnArgToCall();
        }
        else
        {
            WriteReturnArgToCall();
            WriteArgsToCall();
        }
        switch (type)
        {
            case MethodType.Class:
            case MethodType.Utility:
                break;
            case MethodType.Native:
                cppSourceFile.Write($", {argumentCount}");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
        
        cppSourceFile.WriteLine(");");
        
        

        if (argumentCount > 0)
        {
            cppSourceFile.WriteLine("\tdelete[] args;");
        }

        if (hasReturnValue)
        {
            cppSourceFile.WriteLine($"\treturn {string.Format(conversion, "ret")};");
        }

        csFile.WriteLine(csCallPattern, $"GDExtensionInterface.{cppFunctionName.ToPascalCaseWithGodotAbbreviations()}({string.Join(", ", csNativeCallArgs)})");
        
        cppSourceFile.WriteLine("}");
        csFile.WriteLine("\t}");
        csFile.WriteLine();

        void WriteArgsToCall()
        {
            cppSourceFile.Write(cppCallArgs.Count > 0 ? "args" : "nullptr");
        }

        void WriteReturnArgToCall()
        {
            if (hasReturnValue)
            {
                cppSourceFile.Write(canGodotBePassedByValue ? ", &ret" : ", ret.pointer");
            }
            else
            {
                cppSourceFile.Write(", nullptr");
            }
        }
    }

    private void EqualAndHash(string className, StreamWriter file)
    {
        file.WriteLine("\tpublic override bool Equals(object obj) {");
        file.WriteLine("\t\tif (obj == null) { return false; }");
        file.WriteLine($"\t\tif (obj is {Fixer.CSType(className, api).csType} other == false) {{ return false; }}");
        file.WriteLine("\t\treturn this == other;");
        file.WriteLine("\t}");
        file.WriteLine();

        //todo: based on members
        file.WriteLine("\tpublic override int GetHashCode() {");
        file.WriteLine("\t\treturn base.GetHashCode();");
        file.WriteLine("\t}");
        file.WriteLine();
    }

    private Api.Method? GetMethod(string cName, string name)
    {
        foreach (Api.Class c in api.Classes)
            if (cName == c.Name)
            {
                if (c.Methods != null)
                {
                    foreach (Api.Method m in c.Methods!)
                    {
                        if (m.Name == name)
                        {
                            return m;
                        }
                    }
                }

                string? inherits = c.Inherits;
                if (inherits == "Object") inherits = "GodotObject";
                if (inherits != null)
                {
                    return GetMethod(inherits, name);
                }
            }
        return null;
    }

    private void Classes()
    {
        string classesCsDir = csDir + "/Classes";
        string classesCppDir = rsGeneratedDir + "/Classes";
        Directory.CreateDirectory(classesCsDir);
        StreamWriter classesFile = File.CreateText(classesCppDir + "/classes.hpp");
        
        classesFile.WriteLine("""
                              //---------------------------------------------
                              // This file is generated. Changes will be lost
                              //---------------------------------------------
                              #pragma once

                              """);
        
        foreach (Api.Class c in api.Classes)
        {
            switch (c.Name)
            {
                case "int":
                case "float":
                case "bool":
                    break;
                default:
                    Class(c, classesCsDir, classesCppDir, classesFile);
                    break;
            }
        }
        
        classesFile.Close();
    }

    private void Class(Api.Class c, string classesCsDir, string classesCppDir, TextWriter classesFile)
    {
        // TODO
        string className = c.Name;
        switch (className)
        {
            case "GDScriptNativeClass":
            case "JavaClassWrapper":
            case "JavaScriptBridge":
            case "ThemeDB":
                //in 'extension_api' but not in 'ClassDB' at latest init level
                return;
            case "Object":
                className = "GodotObject";
                break;
        }
        
        classesFile.WriteLine($"#include \"{className}.hpp\"");
        
        var classFunctions = new List<string>();
        ClassFunctions[className] = classFunctions;
        
        StreamWriter csFile = File.CreateText(classesCsDir + "/" + className + ".cs");
        StreamWriter cppHeaderFile = File.CreateText(classesCppDir + "/" + className + ".hpp");
        StreamWriter cppSourceFile = File.CreateText(classesCppDir + "/" + className + ".cpp");
        // registrations[c.ApiType].Add(className);
   
        (string csTypeName, bool partial) = Fixer.CSType(className, api);
        WriteRustFileHeader(cppSourceFile);

        cppHeaderFile.WriteLine("namespace GDExtensionInterface {");
        cppHeaderFile.WriteLine("extern \"C\" {");
        cppSourceFile.WriteLine($"#include \"godot_cpp/classes/{className.ToSnakeCaseWithGodotAbbreviations()}.hpp\"");
        cppSourceFile.WriteLine();
        cppSourceFile.WriteLine($"static const godot::StringName class_name = \"{className}\";");
        cppSourceFile.WriteLine("extern \"C\" {");
        
        cppHeaderFile.WriteLine($"GodotType {className.ToSnakeCaseWithGodotAbbreviations()}_constructor();");
        cppSourceFile.WriteLine($"GodotType {className.ToSnakeCaseWithGodotAbbreviations()}_constructor() {{");
        cppSourceFile.WriteLine($"\tauto new_instance = memnew(godot::{className});");
        cppSourceFile.WriteLine("\treturn GodotType { new_instance->_owner };");
        cppSourceFile.WriteLine("}");
        
        cppHeaderFile.WriteLine($"void {className.ToSnakeCaseWithGodotAbbreviations()}_destructor(GodotType p_self);");
        cppSourceFile.WriteLine($"void {className.ToSnakeCaseWithGodotAbbreviations()}_destructor(GodotType p_self) {{");
        cppSourceFile.WriteLine("\tmemdelete(p_self.pointer);");
        cppSourceFile.WriteLine("}");

        Class? doc = GetDocs(className);

        var methodRegistrations = new List<string>();
    
        csFile.WriteLine("namespace GDExtensionInterface;");
        csFile.WriteLine();
        csFile.Write("public unsafe ");
        bool isSingleton = api.Singletons.Any(x => x.Type == className);
        string inherits = c.Inherits ?? "Wrapped";
        if (inherits == "Object")
        {
            inherits = "GodotObject";
        }
        csFile.WriteLine($"partial class {className} : {inherits} {{");
        csFile.WriteLine();
    
    
        if (isSingleton)
        {
            csFile.WriteLine("// TODO: Singleton");
            // csFile.WriteLine($"\tprivate static {className} _singleton = null;");
            // csFile.WriteLine($"\tpublic static {className} Singleton {{");
            // csFile.WriteLine($"\t\tget => _singleton ??= new {className}(GDExtensionInterface.GlobalGetSingleton(__godot_name));");
            // csFile.WriteLine("\t}");
            // csFile.WriteLine();
        }
    
        // TODO
        // if (c.Constants != null)
        // {
        //     foreach (Api.ValueData con in c.Constants)
        //     {
        //         Constant? d = doc?.constants?.FirstOrDefault(x => x.name == con.Name);
        //         if (d is { comment: not null })
        //         {
        //             string com = Fixer.XMLComment(d.comment);
        //             csFile.WriteLine(com);
        //         }
        //         if (con.Name.StartsWith("NOTIFICATION_"))
        //         {
        //             csFile.WriteLine($"\tpublic const Notification {con.Name.ToPascalCase()} = (Notification){con.Value};");
        //         }
        //         else
        //         {
        //             csFile.WriteLine($"\tpublic const int {con.Name} = {con.Value};");
        //         }
        //     }
        //     csFile.WriteLine();
        // }
    
        if (c.Enums != null)
        {
            foreach (Api.Enum e in c.Enums)
            {
                Enum(e, csFile, doc?.constants);
            }
        }
    
        var addedMethods = new List<Api.Method>();
    
        if (c.Properties != null)
        {
            foreach (Api.Property prop in c.Properties)
            {
                string type = prop.Type;
                var cast = "";
    
                Api.Method? getter = GetMethod(className, prop.Getter);
                Api.Method? setter = GetMethod(className, prop.Setter);
    
                if (getter == null && setter == null)
                {
                    var valType = new Api.MethodReturnValue
                    {
                        Meta = type,
                        Type = type,
                    };
                    if (!string.IsNullOrEmpty(prop.Getter))
                    {
                        getter = new Api.Method
                        {
                            Arguments = null,
                            Category = null,
                            Hash = null,
                            Name = prop.Getter,
                            ReturnType = type,
                            ReturnValue = valType,
                            IsStatic = false,
                        };
                        addedMethods.Add(getter.Value);
                    }
                    if (!string.IsNullOrEmpty(prop.Setter))
                    {
                        setter = new Api.Method
                        {
                            Arguments = new Api.Argument[]
                            {
                            new()
                            {
                                DefaultValue = null,
                                Name = "value",
                                Type = type,
                                Meta = type,
                            },
                            },
                            Category = null,
                            Hash = null,
                            Name = prop.Setter,
                            ReturnType = type,
                            ReturnValue = valType,
                            IsStatic = false,
                        };
                        addedMethods.Add(setter.Value);
                    }
                }
                if (doc is { members: not null })
                {
                    Member? d = doc.members.FirstOrDefault(x => x.name == prop.Name);
                    if (d is { comment: not null })
                    {
                        string com = Fixer.XMLComment(d.comment);
                        csFile.WriteLine(com);
                    }
                }
                type = getter != null 
                    ? getter.Value.ReturnValue!.Value.Type 
                    : setter!.Value.Arguments![0].Type;
    
                bool hasEnumOfSameName = (c.Enums?.Where(x => x.Name == Fixer.Name(prop.Name.ToPascalCase())).FirstOrDefault())?.Name != null;
    
                csFile.Write($"\tpublic {Fixer.CSType(type, api).csType} {Fixer.Name(prop.Name.ToPascalCase()) + (hasEnumOfSameName ? "Value" : "")} {{ ");
    
                if (prop.Index.HasValue)
                {
                    if (getter == null)
                    {
                        throw new NotImplementedException("get cast from Setter");
                    }
                    cast = $"({Fixer.CSType(getter.Value.Arguments![0].Type, api).csType})";
                }
    
                if (getter != null)
                {
                    csFile.Write($"get => {Fixer.MethodName(prop.Getter)}(");
                    if (prop.Index.HasValue)
                    {
                        csFile.Write($"{cast}{prop.Index.Value}");
                    }
                    csFile.Write("); ");
                }
    
                if (setter != null)
                {
                    csFile.Write($"set => {Fixer.MethodName(prop.Setter)}(");
                    if (prop.Index.HasValue)
                    {
                        csFile.Write($"{cast}{prop.Index.Value}, ");
                    }
                    csFile.Write("value); ");
                }
                csFile.WriteLine("}");
            }
            csFile.WriteLine();
        }
    
    
        addedMethods.AddRange(c.Methods ?? Array.Empty<Api.Method>());
    
        foreach (Api.Method meth in addedMethods)
        {
            Method? d = null;
            if (doc is { methods: not null })
            {
                d = doc.methods.FirstOrDefault(x => x.name == meth.Name);
            }
            Method(meth, className, csFile,  cppHeaderFile,  cppSourceFile, MethodType.Class, d, classFunctions, isSingleton);
        }
        // TODO
        
        // if (c.Signals != null)
        // {
        //     foreach (Api.Signal sig in c.Signals)
        //     {
        //         csFile.Write($"\tpublic void EmitSignal{sig.Name.ToPascalCase()}(");
        //         if (sig.Arguments != null)
        //         {
        //             for (var j = 0; j < sig.Arguments.Length; j++)
        //             {
        //                 Api.Argument p = sig.Arguments[j];
        //                 csFile.Write($"{Fixer.CSType(p.Type, api).csType} {Fixer.Name(p.Name)}{(j < sig.Arguments.Length - 1 ? ", " : "")}");
        //             }
        //         }
        //
        //         csFile.Write($") => EmitSignal(\"{sig.Name}\"{(sig.Arguments != null ? ", " : "")}");
        //         if (sig.Arguments != null)
        //         {
        //             for (var j = 0; j < sig.Arguments.Length; j++)
        //             {
        //                 Api.Argument p = sig.Arguments[j];
        //                 csFile.Write($"{Fixer.Name(p.Name)}{(j < sig.Arguments.Length - 1 ? ", " : "")}");
        //             }
        //         }
        //         csFile.WriteLine(");");
        //         csFile.WriteLine();
        //         csFile.Write($"\tpublic delegate void Signal{sig.Name.ToPascalCase()}(");
        //         if (sig.Arguments != null)
        //         {
        //             for (var j = 0; j < sig.Arguments.Length; j++)
        //             {
        //                 Api.Argument p = sig.Arguments[j];
        //                 csFile.Write($"{Fixer.CSType(p.Type, api).csType} {Fixer.Name(p.Name)}{(j < sig.Arguments.Length - 1 ? ", " : "")}");
        //             }
        //         }
        //         csFile.WriteLine(");");
        //         csFile.WriteLine();
        //         csFile.WriteLine($"\tpublic event Signal{sig.Name.ToPascalCase()} {sig.Name.ToPascalCase()}{{");
        //         csFile.WriteLine($"\t\tadd => Connect(\"{sig.Name}\", Callable.From(value, this));");
        //         csFile.WriteLine($"\t\tremove => Disconnect(\"{sig.Name}\", Callable.From(value, this));");
        //         csFile.WriteLine("}");
        //         csFile.WriteLine();
        //     }
        //     csFile.WriteLine();
        // }
    
        EqualAndHash(className, csFile);
    
        string content = className == "RefCounted" ? " Reference();\n" : "";
    
        content += "\tRegister();";
    
        csFile.WriteLine($"\tpublic {className}() : base(__godot_name) {{");
        csFile.WriteLine(content);
        csFile.WriteLine("}");
        csFile.WriteLine($"\tprotected {className}(StringName type) : base(type) {{{content}}}");
        csFile.WriteLine($"\tprotected {className}(IntPtr ptr) : base(ptr) {{{content}}}");
        csFile.WriteLine($"\tinternal static {className} Construct(IntPtr ptr) => new (ptr);");
        csFile.WriteLine();
    
        csFile.WriteLine($"\tpublic new static StringName __godot_name => *(StringName*)GDExtensionInterface.CreateStringName(\"{className}\");");
        for (var i = 0; i < methodRegistrations.Count; i++)
        {
            csFile.WriteLine($"\tstatic IntPtr __methodPointer{i} => {methodRegistrations[i]};");
        }
        csFile.WriteLine();
        csFile.WriteLine("\tpublic new static void Register() {");
        csFile.WriteLine($"\t\tif (!RegisterConstructor(\"{className}\", Construct)) return;");
        csFile.WriteLine($"\t\tGDExtensionInterface.{inherits}.Register();");
        csFile.WriteLine("\t}");
        csFile.WriteLine("}");
        csFile.Close();
        cppHeaderFile.WriteLine("}");
        cppHeaderFile.WriteLine("}");
        cppSourceFile.WriteLine("}");

        cppHeaderFile.Close();
        cppSourceFile.Close();
    }

    private void Variant(TextWriter csFile)
    {
        foreach (Api.BuiltinClass builtinClass in api.BuiltinClasses)
        {
            string csType = Fixer.CSType(builtinClass.Name, api).csType;
            if (!NeedsConvert(csType)) continue;
            csFile.Write("\tpublic static implicit operator Variant(");
            csFile.Write(csType);
            string csClassName = Fixer.VariantName(csType);
            csFile.WriteLine($" value) => New{csClassName}(value);");
            csFile.Write("\tpublic static explicit operator ");
            csFile.Write(csType);
            csFile.Write("(Variant value) => value.As");
            csFile.Write(csClassName);
            csFile.WriteLine("();");
        }
        
        csFile.WriteLine();
        return;

        static bool NeedsConvert(string t)
        {
            return t switch
            {
                "bool" => true,
                "int" => true,
                "long" => true,
                "float" => true,
                "double" => true,
                _ => false,
            };
        }
    }
}
