using System.Diagnostics;
using System.Text;
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

    private readonly Dictionary<string, HashSet<string>> registrations = new()
    {
        ["builtin"] = new HashSet<string> { "StringName", "Variant" },
        ["utility"] = new HashSet<string>(),
        ["core"] = new HashSet<string>(),
        ["servers"] = new HashSet<string>(),
        ["scene"] = new HashSet<string>(),
        ["editor"] = new HashSet<string>(),
    };

    private readonly Api api;
    private readonly XmlSerializer classXml = new(typeof(Class));
    private readonly XmlSerializer builtinXml = new(typeof(BuiltinClass));
    private readonly string csDir;
    private readonly string cppGeneratedDir;
    private readonly string? docDir;
    private readonly string configName;
    
    public Convert(Api api, string csDir, string cppGeneratedDir, string docDir, string configName)
    {
        this.api = api;
        this.csDir = csDir;
        this.cppGeneratedDir = cppGeneratedDir;
        this.docDir = docDir;
        this.configName = configName;
        foreach (Api.BuiltinClassSizesOfConfig classSizes in api.BuiltinClassSizes)
        {
            if (classSizes.BuildConfiguration != configName) continue;
            foreach (Api.ClassSize size in classSizes.Sizes)
            {
                BuiltinClassSizes[size.Name] = size.Size;
            }
        }
    }

    public Dictionary<string, List<string>> BuiltinClassFunctions { get; } = new();
    public Dictionary<string, List<string>> ClassFunctions { get; } = new();
    public static readonly Dictionary<string, int> BuiltinClassSizes = new();

    public void Emit()
    {

        foreach (string o in builtinObjectTypes)
        {
            objectTypes.Add(o);
        }

        foreach (Api.Class c in api.Classes)
        {
            objectTypes.Add(c.Name);
        }

        BuiltinClasses();
        Classes();

        Directory.CreateDirectory(csDir + "/Enums");
        foreach (Api.Enum e in api.GlobalEnums)
        {
            GlobalEnum(e, csDir + "/Enums");
        }

        Directory.CreateDirectory(csDir + "/NativeStructures");
        // TODO?
        // foreach (Api.NativeStructure native in api.NativeStructures)
        // {
        //     StreamWriter file = File.CreateText(csDir + "/NativeStructures/" + Fixer.CSType(native.Name, api) + ".cs");
        //     file.WriteLine("namespace GodotSharpGDExtension;");
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
        var files = new Dictionary<string, (StreamWriter csFile, StreamWriter cppHeaderFile, StreamWriter cppSourceFile, List<string> registrations)>();
        foreach (Api.Method f in api.UtilityFunction)
        {
            string category = string.Concat(f.Category![0].ToString().ToUpper(), f.Category.AsSpan(1));
            if (files.TryGetValue(category, out (StreamWriter csFile, StreamWriter cppHeaderFile, StreamWriter cppSourceFile, List<string> registrations) file) == false)
            {
                file = (
                    File.CreateText($"{csDir}/UtilityFunctions/{category}.cs"), 
                    File.CreateText($"{cppGeneratedDir}/UtilityFunctions/{category}.hpp"),
                    File.CreateText($"{cppGeneratedDir}/UtilityFunctions/{category}.cpp"),
                    new List<string>());
                files.Add(category, file);
                file.Item1.WriteLine("namespace GodotSharpGDExtension;");
                file.Item1.WriteLine($"public static unsafe partial class {category} {{");
                registrations["utility"].Add(category);
            }
            Method? d = null;
            if (docGlobalScope is { methods: not null })
            {
                d = docGlobalScope.methods.FirstOrDefault(x => x.name == f.Name);
            }
            Method(f, "", file.csFile, file.cppHeaderFile, file.cppSourceFile, MethodType.Utility, file.registrations, d);
        }

        // TODO
        // Variant();
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
        string name = Fixer.CSType(e.Name, api).Replace(".", "");
        StreamWriter file = File.CreateText(dir + "/" + Fixer.CSType(name, api) + ".cs");
        file.WriteLine("namespace GodotSharpGDExtension {");
        Enum(e, file);
        file.WriteLine("}");
        file.Close();
    }

    private void BuiltinClasses()
    {
        string builtinClassesCsDir = this.csDir + "/BuiltinClasses";
        string builtinClassesCppDir = this.cppGeneratedDir + "/BuiltinClasses";
        Directory.CreateDirectory(builtinClassesCsDir);
        Directory.CreateDirectory(builtinClassesCppDir);
        var variantCsFile = new StreamWriter(Path.Join(builtinClassesCsDir, "Variant.cs"));
        var variantCppHeaderFile = new StreamWriter(Path.Join(builtinClassesCppDir, "Variant.hpp"));
        var variantCppSourceFile = new StreamWriter(Path.Join(builtinClassesCppDir, "Variant.cpp"));

        WriteCppFileHeaders(variantCppHeaderFile, variantCppSourceFile, "Variant");
        variantCppHeaderFile.WriteLine("namespace GodotSharpGDExtension {");
        variantCppHeaderFile.WriteLine("extern \"C\" {");
        variantCppSourceFile.WriteLine("extern \"C\" {");
        variantCsFile.WriteLine("namespace GodotSharpGDExtension;");
        variantCsFile.WriteLine();
        variantCsFile.WriteLine("public partial class Variant {");    

        var generalVariantClassFunctions = new List<string>();
        BuiltinClassFunctions["Variant"] = generalVariantClassFunctions;

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
                case "int":
                case "float":
                case "bool":
                case "String":
                    BuiltinClass? doc = GetBuiltinDocs(c.Name);
                    WriteVariantConversionFunctions(c, variantCsFile, variantCppHeaderFile, variantCppSourceFile,
                        BuiltinClassSizes[c.Name], Fixer.CSType(c.Name, api), generalVariantClassFunctions, api);
                    WriteBuiltinConstructors(c, doc, variantCsFile, variantCppHeaderFile, variantCppSourceFile, BuiltinClassSizes[c.Name], generalVariantClassFunctions,
                        _ => new ValueTuple<string, string>("static Variant New", "variant_"), staticConstructorPattern);
                    break;
                default:
                    BuiltinClass(c, builtinClassesCsDir, builtinClassesCppDir, builtinObjectTypes.Contains(c.Name), variantCsFile, variantCppHeaderFile, variantCppSourceFile, generalVariantClassFunctions);
                        
                    break;
            }
        }
        variantCsFile.WriteLine("}");
        variantCppHeaderFile.WriteLine("}");
        variantCppHeaderFile.WriteLine("}");
        variantCppSourceFile.WriteLine("}");
        variantCsFile.Close();
        variantCppHeaderFile.Close();
        variantCppSourceFile.Close();
    }

    private void BuiltinClass(Api.BuiltinClass builtinClass, string builtinClassesCsDir, string builtinClassesCppDir, bool hasPointer, TextWriter variantCsFile, TextWriter variantCppHeaderFile, TextWriter variantCppSourceFile, ICollection<string> variantClassFunctions)
    {
        string className = builtinClass.Name;
        if (className == "Object") className = "GodotObject";
        string csTypeName = Fixer.CSType(className, api);
        StreamWriter csFile = File.CreateText(builtinClassesCsDir + "/" + csTypeName + ".cs");
        StreamWriter cppHeaderFile = File.CreateText(builtinClassesCppDir + "/" + csTypeName + ".hpp");
        StreamWriter cppSourceFile = File.CreateText(builtinClassesCppDir + "/" + csTypeName + ".cpp");
        
        WriteCppFileHeaders(cppHeaderFile, cppSourceFile, csTypeName);

        registrations["builtin"].Add(csTypeName);

        var classFunctions = new List<string>();
        BuiltinClassFunctions[className] = classFunctions;
        
        BuiltinClass? doc = GetBuiltinDocs(className);

        var methodRegistrations = new List<string>();

        int size = BuiltinClassSizes[builtinClass.Name];

        WriteVariantConversionFunctions(builtinClass, variantCsFile, variantCppHeaderFile, variantCppSourceFile, size, csTypeName, classFunctions, api);

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
        cppHeaderFile.WriteLine("namespace GodotSharpGDExtension {");
        cppHeaderFile.WriteLine("extern \"C\" {");
        cppSourceFile.WriteLine("extern \"C\" {");
        csFile.WriteLine("namespace GodotSharpGDExtension;");
        csFile.WriteLine();
        csFile.WriteLine($"public class {csTypeName} : Variant<{csTypeName}> {{");    
        // TODO: remove?
        // csFile.WriteLine("\tstatic private bool _registered = false;");
        // csFile.WriteLine();

        // TODO: remove?
        // csFile.WriteLine($"\tpublic const int StructSize = {size};");
        csFile.WriteLine($"\tpublic {className}(IntPtr ptr) : base(ptr) {{}}");
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

        if (builtinClass.Members != null)
        {
            foreach (Api.BuiltinMember member in builtinClass.Members)
            {
                Member? d = null;
                if (doc is { members: not null })
                {
                    d = doc.members.FirstOrDefault(x => x.name == member.Name);
                }
                Member(member, csFile, cppHeaderFile, cppSourceFile, className, d, classFunctions);
            }
        }
        
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

        WriteBuiltinConstructors(builtinClass, doc, csFile, cppHeaderFile, cppSourceFile, size, classFunctions);

        if (builtinClass.Operators != null)
        {
            foreach (Api.Operator op in builtinClass.Operators)
            {
                Operator? d = null;
                if (doc is { operators: not null })
                {
                    d = doc.operators.FirstOrDefault(x => x.name == $"operator {op.Name}");
                }
                Operator(op, className, csFile, cppHeaderFile, cppSourceFile, d, classFunctions);
            }
        }

        if (builtinClass.Enums != null)
        {
            foreach (Api.Enum e in builtinClass.Enums)
            {
                Enum(e, csFile, doc?.constants);
            }
        }

        if (builtinClass.Methods != null)
        {
            foreach (Api.Method meth in builtinClass.Methods)
            {
                Method? d = null;
                if (doc is { methods: not null })
                {
                    d = doc.methods.FirstOrDefault(x => x.name == meth.Name);
                }
                Method(meth, className, csFile, cppHeaderFile, cppSourceFile, MethodType.Native, methodRegistrations, d, isBuiltinPointer: hasPointer);
            }
        }

        EqualAndHash(className, csFile);


        var cppDestructorName = $"{builtinClass.Name}_destructor";
        var cppDestructorSignature = $"void {cppDestructorName}(GDExtensionTypePtr p_base)";
        cppHeaderFile.WriteLine($"{cppDestructorSignature};");
        cppSourceFile.WriteLine($"{cppDestructorSignature} {{");
        cppSourceFile.WriteLine($"\tstatic auto destructor_func = godot::internal::gdextension_interface_variant_get_ptr_destructor({builtinClass.VariantName});");
        cppSourceFile.WriteLine("\tdestructor_func(p_base);");
        cppSourceFile.WriteLine("}");
        
        csFile.WriteLine($"\t~{csTypeName}() {{");
        csFile.WriteLine($"\t\tGDExtensionInterface.{csTypeName}Destructor(InternalPointer);");
        csFile.WriteLine("\t}");
        
        classFunctions.Add(cppDestructorName);
        
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
        cppHeaderFile.WriteLine("}");
        cppHeaderFile.WriteLine("}");
        cppSourceFile.WriteLine("}");
        
        cppHeaderFile.Close();
        cppSourceFile.Close();
    }

    private static void WriteVariantConversionFunctions(Api.BuiltinClass builtinClass, TextWriter variantCsFile,
        TextWriter variantCppHeaderFile, TextWriter variantCppSourceFile, int size,
        string csTypeName, ICollection<string> variantClassFunctions, Api api)
    {
        // TODO: Fix CSName and return
        var nativeToFunctionName = $"variant_to_{builtinClass.Name.ToPascalCase()}";
        variantClassFunctions.Add(nativeToFunctionName);

        string godotCppType = Fixer.GodotCppType(builtinClass.Name, api);
        (string typeForDotnetInterop, string conversion, bool isDotnetPod, bool isGodotPod) = Fixer.GetConvertToDotnetDataForType(godotCppType);
        
        var cppToFunctionSignature = $"{typeForDotnetInterop} {{0}}{nativeToFunctionName}(GDExtensionVariantPtr variant)";
        variantCppHeaderFile.WriteLine($"GDE_EXPORT {string.Format(cppToFunctionSignature, "")};");
        variantCppSourceFile.WriteLine($"{string.Format(cppToFunctionSignature, "GodotSharpGDExtension::")} {{");
        variantCppSourceFile.WriteLine(
            $"const auto func = godot::internal::gdextension_interface_get_variant_to_type_constructor({builtinClass.VariantName});");
        variantCppSourceFile.WriteLine(isGodotPod ? $"{godotCppType} new_instance = {{}};" : $"auto new_instance = new uint8_t[{size}];");
        variantCppSourceFile.WriteLine($"func({(isGodotPod ? "&new_instance" : "new_instance")}, variant);");
        variantCppSourceFile.WriteLine($"return {conversion};", "new_instance");
        variantCppSourceFile.WriteLine("}");
        variantCsFile.WriteLine($"\t\tpublic {csTypeName} To{csTypeName.ToPascalCase()}()");
        variantCsFile.WriteLine("\t\t{");
        variantCsFile.WriteLine(
            $"\t\t\treturn {(isDotnetPod ? "" : $"new {csTypeName}(") }GDExtensionInterface.{nativeToFunctionName.ToPascalCase()}(InternalPointer) {(isDotnetPod ? "" : ")")};");
        variantCsFile.WriteLine("\t\t}");
    }

    private void WriteBuiltinConstructors(Api.BuiltinClass c, BuiltinClass? doc, TextWriter csFile,
        TextWriter cppHeaderFile, TextWriter cppSourceFile, int size, ICollection<string> classFunctions, Func<Api.Constructor, (string csConstructorNamePrefix, string cppConstructorNamePrefix)>? getConstructorNamePrefixes = null, string csConstructorPattern = DefaultBuiltinCsConstructorPattern)
    {
        if (c.Constructors != null)
        {
            for (var i = 0; i < c.Constructors.Length; i++)
            {
                Api.Constructor constructor = c.Constructors[i];
                Constructor? d = null;
                if (doc is { constructors: not null })
                {
                    d = doc.constructors[i];
                }

                Constructor(c, constructor, csFile, cppHeaderFile, cppSourceFile, d, size, classFunctions, getConstructorNamePrefixes, csConstructorPattern);
            }
        }
        else
        {
            var emptyApiConstructor = new Api.Constructor { Arguments = Array.Empty<Api.Argument>(), Index = 0 };
            var emptyDocConstructor = new Constructor();
            Constructor(c, emptyApiConstructor, csFile, cppHeaderFile, cppSourceFile, emptyDocConstructor, size,
                classFunctions, getConstructorNamePrefixes, csConstructorPattern);
        }
    }

    private static void WriteCppFileHeaders(TextWriter cppHeaderFile, TextWriter cppSourceFile, string csTypeName)
    {
        cppHeaderFile.WriteLine("""
                                //---------------------------------------------
                                // This file is generated. Changes will be lost
                                //---------------------------------------------
                                #pragma once

                                #include "godot_dotnet.h"
                                #include "gdextension_interface.h"
                                #include "godot_cpp/core/defs.hpp"

                                """);
        cppSourceFile.WriteLine($"""
                                 //---------------------------------------------
                                 // This file is generated. Changes will be lost
                                 //---------------------------------------------

                                 #include "{csTypeName}.hpp"

                                 #include <array>

                                 #include "godot_dotnet.h"
                                 #include "godot_cpp/variant/string_name.hpp"
                                 #include "godot_cpp/godot.hpp"
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
        string csType = Fixer.CSType(builtinMember.Type, api);
        (string memberType, string? conversion, bool isDotnetPod, bool isGodotPod) = Fixer.GetConvertToDotnetDataForType(cppType);

        var getterSignature = $"{memberType} {{0}}{getterName}(GDExtensionTypePtr p_base)";
        var setterSignature = $"void {{0}}{setterName}(GDExtensionTypePtr p_base, {memberType} p_value)";
        var getterCall = $"GDExtensionInterface.{getterName.ToPascalCase()}(InternalPointer)";
        csFile.WriteLine($$"""
                           	public {{csType}} {{builtinMember.Name.ToPascalCase()}}
                           	{
                           		get => {{(isDotnetPod ? getterCall : $"new({getterCall})")}};
                           		set => GDExtensionInterface.{{setterName.ToPascalCase()}}(InternalPointer, {{(isDotnetPod ? "value" : "value.InternalPointer")}});
                           	}
                           """);
        cppHeaderFile.WriteLine($"""
                                 GDE_EXPORT {string.Format(getterSignature, "")};
                                 GDE_EXPORT {string.Format(setterSignature, "")};
                                 """);

        cppSourceFile.WriteLine();
        cppSourceFile.WriteLine($"{string.Format(getterSignature, "GodotSharpGDExtension::")} {{");
        string variantEnumType = className.VariantEnumType();
        cppSourceFile.WriteLine($"\tstatic auto func = godot::internal::gdextension_interface_variant_get_ptr_getter({variantEnumType}, godot::StringName(\"{builtinMember.Name}\")._native_ptr());");
        cppSourceFile.WriteLine(
            isGodotPod 
                ? $"\t{memberType} value = {{}};"
                : $"\tauto value = godot::internal::gdextension_interface_mem_alloc({BuiltinClassSizes[builtinMember.Type]});");
        cppSourceFile.WriteLine(
            isGodotPod
            ? "\tfunc(p_base, &value);"
            : "\tfunc(p_base, value);");
        cppSourceFile.WriteLine($"\treturn {conversion};", "value");
        cppSourceFile.WriteLine("}");

        cppSourceFile.WriteLine($"{string.Format(setterSignature, "GodotSharpGDExtension::")} {{");
        cppSourceFile.WriteLine($"\tstatic auto func = godot::internal::gdextension_interface_variant_get_ptr_setter({variantEnumType}, godot::StringName(\"{builtinMember.Name}\")._native_ptr());");
        cppSourceFile.WriteLine(
            isGodotPod 
                ? "\tfunc(p_base, &p_value);"
                : "\tfunc(p_base, p_value);");
        cppSourceFile.WriteLine("}");        
        classFunctions.Add(getterName);
        classFunctions.Add(setterName);
        csFile.WriteLine();
    }

    private void Constructor(Api.BuiltinClass c, Api.Constructor constructor, TextWriter csFile, TextWriter cppHeaderFile,
        TextWriter cppSourceFile, Constructor? doc,
        int size, ICollection<string> classFunctions, Func<Api.Constructor, (string csConstructorNamePrefix, string cppConstructorNamePrefix)>? getConstructorNamePrefixes, string csConstructorPattern)
    {
        (string csConstructorNamePrefix, string cppConstructorNamePrefix)? prefixResult = getConstructorNamePrefixes?.Invoke(constructor);

        if (doc != null)
        {
            string com = Fixer.XMLComment(doc.description);
            csFile.WriteLine(com);
        }
        var csArgs = new List<string>();
        var nativeArgs = new List<string>();
        var csArgPasses = new List<string>();
        var nativeArgPasses = new List<string>();
        var nativeArgConversions = new List<string>();
        
        if (constructor.Arguments != null)
        {
            foreach (Api.Argument arg in constructor.Arguments)
            {
                string cppType = Fixer.GodotCppType(arg.Type, api);
                (string type, string? conversion, bool isInputPod, bool isConvertedPod) = Fixer.GetConvertFromDotnetDataForType(cppType);

                string csArgName = Fixer.Name(arg.Name).ToCamelCase();
                csArgs.Add($"{Fixer.CSType(arg.Type, api)} {csArgName}");
                var cppArg = $"p_{arg.Name}"; 
                nativeArgs.Add($"{type} {cppArg}");
                csArgPasses.Add(isInputPod ? csArgName : $"{csArgName}.InternalPointer");
                nativeArgConversions.Add($"auto {arg.Name} = {string.Format(conversion, cppArg)}");
                nativeArgPasses.Add(isConvertedPod ? $"&{arg.Name}" : $"{arg.Name}");
            }
        }
        const string argSeparator = ", ";

        var nativeFunctionName = $"{prefixResult?.cppConstructorNamePrefix}{c.Name}_constructor_{constructor.Index}";
        
        var cppFunctionSignature = $"GDExtensionTypePtr {{0}}{nativeFunctionName}({string.Join(argSeparator, nativeArgs)})";
        
        cppHeaderFile.WriteLine($"GDE_EXPORT {string.Format(cppFunctionSignature, "")};");
        cppHeaderFile.WriteLine();
        
        
        // static auto constructor = godot::internal::gdextension_interface_variant_get_ptr_constructor(GDEXTENSION_VARIANT_TYPE_AABB, 1);
        // auto base = new uint8_t[8];
        // std::array<GDExtensionConstTypePtr, 1> call_args = {from};
        // constructor(base, call_args.data());
        // return base;

        cppSourceFile.WriteLine($"{string.Format(cppFunctionSignature, "GodotSharpGDExtension::")} {{");
        cppSourceFile.WriteLine($"\tstatic auto constructor = godot::internal::gdextension_interface_variant_get_ptr_constructor({c.VariantName}, {constructor.Index});");
        cppSourceFile.WriteLine($"\tauto new_instance = new uint8_t[{size}];");
        foreach (string argConversion in nativeArgConversions)
        {
            cppSourceFile.WriteLine($"\t{argConversion};");
        }
        if (nativeArgPasses.Any())
        {
            cppSourceFile.WriteLine($"\tstd::array<GDExtensionConstTypePtr, {nativeArgPasses.Count}> call_args = {{{string.Join(argSeparator, nativeArgPasses)}}};");
        }
        cppSourceFile.Write("\tconstructor(new_instance");
        cppSourceFile.WriteLine(nativeArgPasses.Any() ? ", call_args.data());" : ", nullptr);");
        cppSourceFile.WriteLine("\treturn new_instance;");
        cppSourceFile.WriteLine("}");
        cppSourceFile.WriteLine();
        
        
        // csFile.Write($"\tpublic {prefixResult?.csConstructorNamePrefix}{Fixer.CSType(c.Name, api).ToPascalCase()}(");
        // csFile.Write(string.Join(argSeparator, csArgs));
        // csFile.WriteLine(") {");

        var csFunctionSignature = $"{prefixResult?.csConstructorNamePrefix}{Fixer.CSType(c.Name, api).ToPascalCase()}({string.Join(argSeparator, csArgs)})";
        
        var csCallText = $"GDExtensionInterface.{nativeFunctionName.ToPascalCase()}({string.Join(argSeparator, csArgPasses)})";
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
            
        (string returnTypeToDotnet, string returnConversion, bool isDotnetPod, bool isGodotPod) = Fixer.GetConvertToDotnetDataForType(godotCppReturnType);

        (string leftTypeFromDotnet, string leftConversion, bool isLeftInputTypePod, bool isLeftConvertedTypePod) = Fixer.GetConvertFromDotnetDataForType(godotCppLeftType);

        string csReturnType = Fixer.CSType(op.ReturnType, api);
        string variantEnumType = className.VariantEnumType();
        string? rightClassName = op.RightType;
        if (rightClassName != null)
        {
            if (rightClassName == "Variant") { return; }
            
            string godotCppRightType = Fixer.GodotCppType(rightClassName, api);
            (string rightTypeFromDotnet, string rightConversion, bool isRightInputTypePod, bool isRightConvertedTypePod) = Fixer.GetConvertFromDotnetDataForType(godotCppRightType);

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
            
            cppSourceFile.WriteLine($"{string.Format(cppFunctionSignature, "GodotSharpGDExtension::")} {{");
            cppSourceFile.WriteLine($"\tauto left = {string.Format(leftConversion, "p_left")};");
            cppSourceFile.WriteLine($"\tauto right = {string.Format(rightConversion, "p_right")};");
            cppSourceFile.WriteLine($"\tstatic auto operator_func = godot::internal::gdextension_interface_variant_get_ptr_operator_evaluator({op.Name.VariantOperatorEnum()}, {variantEnumType}, {rightClassName.VariantEnumType()});");
            cppSourceFile.WriteLine(isGodotPod ? $"\t{godotCppReturnType} ret = {{}};" : $"\tauto ret = new uint8_t[{BuiltinClassSizes[op.ReturnType]}];");
            cppSourceFile.WriteLine($"\toperator_func({(isLeftConvertedTypePod ? "&left" : "left")}, {(isRightConvertedTypePod ? "&right" : "right")}, {(isDotnetPod ? "&ret" : "ret")});");
            
            classFunctions.Add(nativeFunctionName);
            
            csFile.WriteLine($"\tpublic static {csReturnType} {name}({Fixer.CSType(className, api)} left, {Fixer.CSType(rightClassName, api)} right) {{");
            csFile.WriteLine($"\t\t{(isDotnetPod ? csReturnType : "IntPtr")} result = GDExtensionInterface.{nativeFunctionName.ToPascalCase()}({(isLeftInputTypePod ? "left" : "left.InternalPointer")}, {(isRightInputTypePod ? "right" : "right.InternalPointer")});");
            csFile.WriteLine($"\t\treturn {(isDotnetPod ? "result" : $"new {csReturnType}(result)")};");            
            
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
            
            cppSourceFile.WriteLine($"{string.Format(cppFunctionSignature, "GodotSharpGDExtension::")} {{");
            cppSourceFile.WriteLine($"\tstatic auto operator_func = godot::internal::gdextension_interface_variant_get_ptr_operator_evaluator({op.Name.VariantOperatorEnum()}, {variantEnumType}, {"NIL".VariantEnumType()});");
            cppSourceFile.WriteLine(isGodotPod ? $"\t{godotCppReturnType} ret = {{}};" : $"\tauto ret = new uint8_t[{BuiltinClassSizes[op.ReturnType]}];");
            cppSourceFile.WriteLine($"\tauto left = {string.Format(leftConversion, "p_left")};");
            cppSourceFile.WriteLine($"\toperator_func({(isLeftConvertedTypePod ? "&left" : "left")}, nullptr, {(isDotnetPod ? "&ret" : "ret")});");
            
            classFunctions.Add(nativeFunctionName);
            
            csFile.WriteLine($"\tpublic static {csReturnType} {name}({Fixer.CSType(className, api)} left) {{");
            csFile.WriteLine($"\t\t{(isDotnetPod ? csReturnType : "IntPtr")} result = GDExtensionInterface.{nativeFunctionName.ToPascalCase()}({(isLeftInputTypePod ? "left" : "left.InternalPointer")});");
            csFile.WriteLine($"\t\treturn {(isDotnetPod ? "result" : $"new {csReturnType}(result)")};");
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

        file.WriteLine($"\tpublic enum {Fixer.CSType(e.Name, api)} {{");
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
        string f = Fixer.CSType(type, api);
        if (type == "String")
        {
            return $"StringMarshall.ToNative({name})";
        }

        if (objectTypes.Contains(type) || builtinObjectTypes.Contains(f))
        {
            return $"{name}.internalPointer";
        }
        return $"(IntPtr)(&{Fixer.Name(name)})";
    }

    private string ReturnStatementValue(string type)
    {
        string f = Fixer.CSType(type, api);
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
        return $"({Fixer.CSType(type, api)}){value}";
    }

    private void Method(Api.Method meth, string className, TextWriter csFile, TextWriter cppHeaderFile, TextWriter cppSourceFile, MethodType type, List<string> methodRegistrations, Method? doc, bool isSingleton = false, bool isBuiltinPointer = false)
    {
        return;
        // TODO
        
//     int64_t Color_to_abgr32(GDExtensionTypePtr p_base) {
//      const auto method = godot::internal::gdextension_interface_variant_get_ptr_builtin_method(GDEXTENSION_VARIANT_TYPE_COLOR, "to_abgr", 0);
//      int64_t ret = {};
//      method(p_base, nullptr, &ret, 0);
//      return ret;
//     }
//
// GDExtensionTypePtr Color_lerp(GDExtensionTypePtr p_base, GDExtensionTypePtr to, float weight) {
//      const auto method = godot::internal::gdextension_interface_variant_get_ptr_builtin_method(GDEXTENSION_VARIANT_TYPE_COLOR, "lerp", 0);
//      std::array<GDExtensionTypePtr, 2> args = {to, &weight};
//      auto ret = new uint8_t[8];
//      method(p_base, args.data(), ret, 2);
//      return ret;
//     }        

// TODO: Debug
        csFile = new StringWriter();
        cppHeaderFile = new StringWriter();
        cppSourceFile = new StringWriter();

        string cppFunctionSignature;

        var csFunctionSignature = "";
        if (doc is { description: not null })
        {
            csFunctionSignature += Fixer.XMLComment(doc.description) + Environment.NewLine;
        }
        csFunctionSignature += "\tpublic ";
        string ret = meth.ReturnType ?? meth.ReturnValue?.Type ?? "";
        if ((meth.IsStatic ?? false) || type == MethodType.Utility || isSingleton)
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

        var csBody = new StringBuilder();


        var csArguments = new List<string>();
        var cppArguments = new List<string>();
        var csArgumentNames = new List<string>();
        var cppArgumentNames = new List<string>();
        var csNativeCallArgs = new List<string>();


        if (meth.Arguments != null)
        {
            for (var i = 0; i < meth.Arguments.Length; i++)
            {
                Api.Argument arg = meth.Arguments[i];
                var suffix = "";
                string argumentName = meth.Arguments[i].Name;
                cppArguments.Add($"{Fixer.GodotCppType(meth.Arguments[i].Type, api)} {argumentName}");
                if (arg.DefaultValue != null)
                {
                    var validDefault = true;
                    for (int j = i; j < meth.Arguments.Length; j++)
                    {
                        validDefault &= IsValidDefaultValue(meth.Arguments[j].DefaultValue!, meth.Arguments[j].Type);
                    }

                    if (validDefault)
                    {
                        csArguments.Add($"{Fixer.CSType(arg.Type, api)} {argumentName} = {FixDefaultValue(arg.DefaultValue, arg.Type)}");
                    }
                    else
                    {
                        csArguments.Add($"{Fixer.CSType(arg.Type, api)} {argumentName}");
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
                // if (i != 0)
                // {
                //     header += ", ";
                // }
                // header += $"{Fixer.CSType(arg.Type, api)} {Fixer.Name(arg.Name)}{suffix}";
                bool isArgPod = Fixer.IsPod(meth.Arguments[i].Type);
                csNativeCallArgs.Add(isArgPod ? argumentName : $"{argumentName}.InternalPointer");
            }
        }
        if (meth.IsVararg)
        {
            csArguments.Add("params object[] parameters");
            csArgumentNames.Add("parameters");
            
            cppArguments.Add("GDExtensionTypePtr varargs[]");
            cppArguments.Add("int vararg_count");
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
        
        if (!string.IsNullOrEmpty(ret))
        {
            csFunctionSignature += $"{Fixer.CSType(ret, api)}";
            cppFunctionSignature = $"{Fixer.GodotCppType(ret, api)}";
        }
        else
        {
            csFunctionSignature += "void";
            cppFunctionSignature = "void";
        }

        var cppFunctionName = $"{className}_{meth.Name}";
        cppFunctionSignature += $" {cppFunctionName}({string.Join(", ", cppArguments)})";
        csFunctionSignature += $" {Fixer.MethodName(meth.Name)}({string.Join(", ", csArguments)})";

        csFile.Write($"{csFunctionSignature} {{");
        
        var m = "";
        if (meth.IsVararg)
        {
            // csNativeCallArgs.Add("varArgs");
            // csFile.WriteLine("var varArgs = new IntPtr[parameters.Length];");
            //
            // csFile.WriteLine($"\t\tfor (var i = 0; i < parameters.Length; i++) {{");
            // csFile.WriteLine("\t\tif (parameters[i] is IGodotType godotType)");
            // csFile.WriteLine("\t\t\tvarArgs[i] = godotType.InternalPointer;");
            // // csFile.WriteLine("\t\t");
            // csFile.WriteLine($"\t\t\t__args[{offset}i] = arguments[i].internalPointer;");
            // csFile.WriteLine("\t\t};");
        }
        if (meth.IsStatic == false && type == MethodType.Native && isBuiltinPointer == false)
        {
            csFile.WriteLine($"\t\tvar __temp = this;");
        }
        if (type == MethodType.Class)
        {

            string call;
            if (meth.IsVararg)
            {
                call = $"\t\tGDExtensionInterface.ObjectMethodBindCall({m}, ";
            }
            else
            {
                string resString = 
                    ret != "" 
                    ? "IntPtr __res = " 
                    : "";
                call = $"\t\t{resString}GDExtensionInterface.ObjectMethodBindPtrcall({m}, ";
            }

            csFile.Write(call);
        }
        else if (type == MethodType.Utility)
        {
            csFile.Write($"\t\tIntPtr __res =GDExtensionInterface.CallGDExtensionPtrUtilityFunction({m}");
        }
        else
        {
            csFile.Write($"\t\tIntPtr __res = GDExtensionInterface.CallGDExtensionPtrBuiltInMethod({m}, ");
        }

        if (type != MethodType.Utility)
        {
            if (meth.IsStatic ?? false)
            {
                csFile.Write("IntPtr.Zero");
            }
            else if (type == MethodType.Class)
            {
                csFile.Write(value: $"{(isSingleton ? "Singleton" : "this")}.internalPointer");
            }
            else if (isBuiltinPointer)
            {
                csFile.Write("this.internalPointer");
            }
            else
            {
                csFile.Write("(IntPtr)(&__temp)");
            }
        }

        if (meth.Arguments != null || meth.IsVararg)
        {
            csFile.Write(", *__args");
        }
        else
        {
            csFile.Write(", IntPtr.Zero");
        }
        if (type == MethodType.Class && meth.IsVararg)
        {
            csFile.Write($", {(meth.Arguments != null ? $"{meth.Arguments.Length} + " : "")}arguments.Length");
        }

        if (type != MethodType.Class)
        {
            csFile.Write(", ");
            if (meth.IsVararg)
            {
                csFile.Write($"{(meth.Arguments != null ? $"{meth.Arguments.Length} + " : "")}arguments.Length");
            }
            else if (meth.Arguments != null)
            {
                csFile.Write($"{meth.Arguments.Length}");
            }
            else
            {
                csFile.Write("0");
            }
        }
        if (type == MethodType.Class && meth.IsVararg)
        {
            csFile.Write(", out IntPtr __res");
            // file.Write(", out GDExtensionCallError __err");
            csFile.Write(", out GDExtensionCallError _");
            
        }
        csFile.WriteLine(");");
        if (ret != "")
        {
            csFile.WriteLine($"\t\treturn {ReturnStatementValue(ret)};");
        }
        csFile.WriteLine("\t}");
        csFile.WriteLine();
    }

    private void EqualAndHash(string className, StreamWriter file)
    {
        file.WriteLine("\tpublic override bool Equals(object obj) {");
        file.WriteLine("\t\tif (obj == null) { return false; }");
        file.WriteLine($"\t\tif (obj is {Fixer.CSType(className, api)} other == false) {{ return false; }}");
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
        //TODO
        return;
        
        // string dir = csDir + "/Classes";
        // Directory.CreateDirectory(dir);
        // foreach (Api.Class c in api.Classes)
        // {
        //     switch (c.Name)
        //     {
        //         case "int":
        //         case "float":
        //         case "bool":
        //             break;
        //         default:
        //             Class(c, dir);
        //             break;
        //     }
        // }
    }

    // private void Class(Api.Class c, string dir)
    // {
    //     // TODO
    //     string className = c.Name;
    //     switch (className)
    //     {
    //         case "GDScriptNativeClass":
    //         case "JavaClassWrapper":
    //         case "JavaScriptBridge":
    //         case "ThemeDB":
    //             //in 'extension_api' but not in 'ClassDB' at latest init level
    //             return;
    //         case "Object":
    //             className = "GodotObject";
    //             break;
    //     }
    //
    //     StreamWriter file = File.CreateText(dir + "/" + className + ".cs");
    //     registrations[c.ApiType].Add(className);
    //
    //     Class? doc = GetDocs(className);
    //
    //     var methodRegistrations = new List<string>();
    //
    //     file.WriteLine("namespace GodotSharpGDExtension;");
    //     file.WriteLine();
    //     file.Write("public unsafe ");
    //     bool isSingleton = api.Singletons.Any(x => x.Type == className);
    //     string inherits = c.Inherits ?? "Wrapped";
    //     if (inherits == "Object")
    //     {
    //         inherits = "GodotObject";
    //     }
    //     file.WriteLine($"partial class {className} : {inherits} {{");
    //     file.WriteLine();
    //     file.WriteLine("\tprivate static bool _registered = false;");
    //     file.WriteLine();
    //
    //
    //     if (isSingleton)
    //     {
    //         file.WriteLine($"\tprivate static {className} _singleton = null;");
    //         file.WriteLine($"\tpublic static {className} Singleton {{");
    //         file.WriteLine($"\t\tget => _singleton ??= new {className}(GDExtensionInterface.GlobalGetSingleton(__godot_name.internalPointer));");
    //         file.WriteLine("\t}");
    //         file.WriteLine();
    //     }
    //
    //     if (c.Constants != null)
    //     {
    //         foreach (Api.ValueData con in c.Constants)
    //         {
    //             Constant? d = doc?.constants?.FirstOrDefault(x => x.name == con.Name);
    //             if (d is { comment: not null })
    //             {
    //                 string com = Fixer.XMLComment(d.comment);
    //                 file.WriteLine(com);
    //             }
    //             if (con.Name.StartsWith("NOTIFICATION_"))
    //             {
    //                 file.WriteLine($"\tpublic const Notification {con.Name.ToPascalCase()} = (Notification){con.Value};");
    //             }
    //             else
    //             {
    //                 file.WriteLine($"\tpublic const int {con.Name} = {con.Value};");
    //             }
    //         }
    //         file.WriteLine();
    //     }
    //
    //     if (c.Enums != null)
    //     {
    //         foreach (Api.Enum e in c.Enums)
    //         {
    //             Enum(e, file, doc?.constants);
    //         }
    //     }
    //
    //     var addedMethods = new List<Api.Method>();
    //
    //     if (c.Properties != null)
    //     {
    //         foreach (Api.Property prop in c.Properties)
    //         {
    //             string type = prop.Type;
    //             var cast = "";
    //
    //             Api.Method? getter = GetMethod(className, prop.Getter);
    //             Api.Method? setter = GetMethod(className, prop.Setter);
    //
    //             if (getter == null && setter == null)
    //             {
    //                 var valType = new Api.MethodReturnValue
    //                 {
    //                     Meta = type,
    //                     Type = type,
    //                 };
    //                 if (!string.IsNullOrEmpty(prop.Getter))
    //                 {
    //                     getter = new Api.Method
    //                     {
    //                         Arguments = null,
    //                         Category = null,
    //                         Hash = null,
    //                         Name = prop.Getter,
    //                         ReturnType = type,
    //                         ReturnValue = valType,
    //                         IsStatic = false,
    //                     };
    //                     addedMethods.Add(getter.Value);
    //                 }
    //                 if (!string.IsNullOrEmpty(prop.Setter))
    //                 {
    //                     setter = new Api.Method
    //                     {
    //                         Arguments = new Api.Argument[]
    //                         {
    //                         new()
    //                         {
    //                             DefaultValue = null,
    //                             Name = "value",
    //                             Type = type,
    //                             Meta = type,
    //                         },
    //                         },
    //                         Category = null,
    //                         Hash = null,
    //                         Name = prop.Setter,
    //                         ReturnType = type,
    //                         ReturnValue = valType,
    //                         IsStatic = false,
    //                     };
    //                     addedMethods.Add(setter.Value);
    //                 }
    //             }
    //             if (doc is { members: not null })
    //             {
    //                 Member? d = doc.members.FirstOrDefault(x => x.name == prop.Name);
    //                 if (d is { comment: not null })
    //                 {
    //                     string com = Fixer.XMLComment(d.comment);
    //                     file.WriteLine(com);
    //                 }
    //             }
    //             type = getter != null 
    //                 ? getter.Value.ReturnValue!.Value.Type 
    //                 : setter!.Value.Arguments![0].Type;
    //
    //             bool hasEnumOfSameName = (c.Enums?.Where(x => x.Name == Fixer.Name(prop.Name.ToPascalCase())).FirstOrDefault())?.Name != null;
    //
    //             file.Write($"\tpublic {Fixer.CSType(type, api)} {Fixer.Name(prop.Name.ToPascalCase()) + (hasEnumOfSameName ? "Value" : "")} {{ ");
    //
    //             if (prop.Index.HasValue)
    //             {
    //                 if (getter == null)
    //                 {
    //                     throw new NotImplementedException("get cast from Setter");
    //                 }
    //                 cast = $"({Fixer.CSType(getter.Value.Arguments![0].Type, api)})";
    //             }
    //
    //             if (getter != null)
    //             {
    //                 file.Write($"get => {Fixer.MethodName(prop.Getter)}(");
    //                 if (prop.Index.HasValue)
    //                 {
    //                     file.Write($"{cast}{prop.Index.Value}");
    //                 }
    //                 file.Write("); ");
    //             }
    //
    //             if (setter != null)
    //             {
    //                 file.Write($"set => {Fixer.MethodName(prop.Setter)}(");
    //                 if (prop.Index.HasValue)
    //                 {
    //                     file.Write($"{cast}{prop.Index.Value}, ");
    //                 }
    //                 file.Write("value); ");
    //             }
    //             file.WriteLine("}");
    //         }
    //         file.WriteLine();
    //     }
    //
    //
    //     addedMethods.AddRange(c.Methods ?? Array.Empty<Api.Method>());
    //
    //     foreach (Api.Method meth in addedMethods)
    //     {
    //         Method? d = null;
    //         if (doc is { methods: not null })
    //         {
    //             d = doc.methods.FirstOrDefault(x => x.name == meth.Name);
    //         }
    //         Method(meth, className, file, MethodType.Class, d, isSingleton: isSingleton);
    //     }
    //     if (c.Signals != null)
    //     {
    //         foreach (Api.Signal sig in c.Signals)
    //         {
    //             file.Write($"\tpublic void EmitSignal{sig.Name.ToPascalCase()}(");
    //             if (sig.Arguments != null)
    //             {
    //                 for (var j = 0; j < sig.Arguments.Length; j++)
    //                 {
    //                     Api.Argument p = sig.Arguments[j];
    //                     file.Write($"{Fixer.CSType(p.Type, api)} {Fixer.Name(p.Name)}{(j < sig.Arguments.Length - 1 ? ", " : "")}");
    //                 }
    //             }
    //
    //             file.Write($") => EmitSignal(\"{sig.Name}\"{(sig.Arguments != null ? ", " : "")}");
    //             if (sig.Arguments != null)
    //             {
    //                 for (var j = 0; j < sig.Arguments.Length; j++)
    //                 {
    //                     Api.Argument p = sig.Arguments[j];
    //                     file.Write($"{Fixer.Name(p.Name)}{(j < sig.Arguments.Length - 1 ? ", " : "")}");
    //                 }
    //             }
    //             file.WriteLine(");");
    //             file.WriteLine();
    //             file.Write($"\tpublic delegate void Signal{sig.Name.ToPascalCase()}(");
    //             if (sig.Arguments != null)
    //             {
    //                 for (var j = 0; j < sig.Arguments.Length; j++)
    //                 {
    //                     Api.Argument p = sig.Arguments[j];
    //                     file.Write($"{Fixer.CSType(p.Type, api)} {Fixer.Name(p.Name)}{(j < sig.Arguments.Length - 1 ? ", " : "")}");
    //                 }
    //             }
    //             file.WriteLine(");");
    //             file.WriteLine();
    //             file.WriteLine($"\tpublic event Signal{sig.Name.ToPascalCase()} {sig.Name.ToPascalCase()}{{");
    //             file.WriteLine($"\t\tadd => Connect(\"{sig.Name}\", Callable.From(value, this));");
    //             file.WriteLine($"\t\tremove => Disconnect(\"{sig.Name}\", Callable.From(value, this));");
    //             file.WriteLine("}");
    //             file.WriteLine();
    //         }
    //         file.WriteLine();
    //     }
    //
    //     EqualAndHash(className, file);
    //
    //     string content = className == "RefCounted" ? " Reference();\n" : "";
    //
    //     content += "\tRegister();";
    //
    //     file.WriteLine($"\tpublic {className}() : base(__godot_name) {{");
    //     file.WriteLine(content);
    //     file.WriteLine("}");
    //     file.WriteLine($"\tprotected {className}(StringName type) : base(type) {{{content}}}");
    //     file.WriteLine($"\tprotected {className}(IntPtr ptr) : base(ptr) {{{content}}}");
    //     file.WriteLine($"\tinternal static {className} Construct(IntPtr ptr) => new (ptr);");
    //     file.WriteLine();
    //
    //     file.WriteLine($"\tpublic new static StringName __godot_name => *(StringName*)GDExtensionInterface.CreateStringName(\"{className}\");");
    //     for (var i = 0; i < methodRegistrations.Count; i++)
    //     {
    //         file.WriteLine($"\tstatic IntPtr __methodPointer{i} => {methodRegistrations[i]};");
    //     }
    //     file.WriteLine();
    //     file.WriteLine("\tpublic new static void Register() {");
    //     file.WriteLine($"\t\tif (!RegisterConstructor(\"{className}\", Construct)) return;");
    //     file.WriteLine($"\t\tGodotSharpGDExtension.{inherits}.Register();");
    //     file.WriteLine("\t}");
    //     file.WriteLine("}");
    //     file.Close();
    // }

    private void Variant()
    {
        Api.Enum type = default;
        Api.Enum operators = default;
        foreach (Api.Enum e in api.GlobalEnums)
        {
            switch (e.Name)
            {
                case "Variant.Type":
                    type = e;
                    break;
                case "Variant.Operator":
                    operators = e;
                    break;
            }
        }
        type.Name = "Type";
        operators.Name = "Operator";
        StreamWriter file = File.CreateText(csDir + "/" + "Variant.cs");
        file.WriteLine("namespace GodotSharpGDExtension;");
        file.WriteLine();
        file.WriteLine("public sealed unsafe partial class Variant {");
        file.WriteLine();

        var types = new string[type.Values.Length - 1];

        foreach (Api.Enum e in new[] { type, operators })
        {
            int prefixLength = Fixer.SharedPrefixLength(e.Values.Select(x => x.Name).ToArray());

            file.WriteLine($"\tpublic enum {Fixer.CSType(e.Name, api)} {{");
            for (var i = 0; i < e.Values.Length; i++)
            {
                Api.ValueData v = e.Values[i];

                string name = v.Name[prefixLength..].ToPascalCase();
                name = name switch
                {
                    "Aabb" => "AABB",
                    "Rid" => "RID",
                    "Object" => "GodotObject",
                    _ => name,
                };
                if (i < types.Length && e == type)
                {
                    types[i] = name;
                }
                file.WriteLine($"\t\t{name} = {v.Value},");
            }
            file.WriteLine("\t}");
            file.WriteLine();
        }

        static string VariantTypeToCSharpType(string t)
        {
            return t switch
            {
                "Bool" => "bool",
                "Int" => "long",
                "Float" => "double",
                "String" => "string",
                _ => t
            };
        }

        file.WriteLine("\tpublic static Variant ObjectToVariant(object value)\n\t{");
        file.WriteLine("\t\tvar valuetype = value?.GetType();");
        file.WriteLine("\t\tif(value is null) { \n\t\t\treturn null; \n\t\t}");
        for (var i = 1; i < types.Length; i++)
        {
            string t = types[i];
            file.WriteLine($"\t\telse if (typeof({VariantTypeToCSharpType(t)}) == valuetype) \n\t\t{{");
            file.WriteLine($"\t\t\treturn new Variant(({VariantTypeToCSharpType(t)})value);");
            file.WriteLine("\t\t}");
        }
        file.WriteLine("\t\telse \n\t\t{ \n\t\t\treturn null; \n\t\t} \n\t}");

        file.WriteLine("\tpublic static object VariantToObject(Variant value)\n\t{");
        file.WriteLine("\t\tvar valuetype = value?.NativeType;");
        file.WriteLine("\t\tif(value is null) { \n\t\t\treturn null; \n\t\t}");
        for (var i = 1; i < types.Length; i++)
        {
            string t = types[i];
            if (t == "Object") t = "GodotObject";
            file.WriteLine($"\t\telse if (Variant.Type.{t} == valuetype) \n\t\t{{");
            file.WriteLine($"\t\t\treturn (object)({VariantTypeToCSharpType(t)})value;");
            file.WriteLine("\t\t}");
        }
        file.WriteLine("\t\telse \n\t\t{ \n\t\t\treturn null; \n\t\t} \n\t}");


        for (var i = 1; i < types.Length; i++)
        {
            string t = types[i];
            if (t == "GodotObject") { continue; }
            file.Write("\tpublic static void SaveIntoPointer(");
            file.Write(VariantTypeToCSharpType(t));
            file.Write(" value, IntPtr ptr) => GDExtensionInterface.CallGDExtensionPtrConstructor(Constructors[(int)Type.");
            file.Write(t);
            file.Write("].fromType, ptr, ");
            if (t == "String")
            {
                file.Write("StringMarshall.ToNative(value)");
            }
            else if (objectTypes.Contains(t))
            {
                file.Write("value.internalPointer");
            }
            else
            {
                file.Write("(IntPtr)(&value)");
            }
            file.WriteLine(");");
        }
        file.WriteLine();

        for (var i = 1; i < types.Length; i++)
        {
            string t = types[i];
            file.Write("\tpublic Variant(");
            file.Write(VariantTypeToCSharpType(t));
            file.WriteLine(" value) : this() => SaveIntoPointer(value, internalPointer);");
        }
        file.WriteLine();

        for (var i = 1; i < types.Length; i++)
        {
            string t = types[i];
            if (t == "GodotObject") { continue; }
            file.Write("\tpublic static ");
            file.Write(VariantTypeToCSharpType(t));
            file.Write(" Get");
            file.Write(t);
            file.WriteLine("FromVariant(Variant data) {");
            file.WriteLine("\t\tvar ptr = data.internalPointer;");
            file.WriteLine($"\t\tif (data.NativeType != Variant.Type.{t}) {{");
            file.WriteLine($"\t\t\treturn default;");
            file.WriteLine("\t\t}");            
            file.WriteLine("\t\tIntPtr __res;");
            file.Write("\t\tGDExtensionInterface.CallGDExtensionPtrConstructor(Constructors[(int)Type.");
            file.Write(t);
            file.WriteLine("].toType, (IntPtr)(&__res), ptr);");
            file.Write("\t\treturn ");
            file.Write(
                t == "String"
                ? "StringMarshall.ToManaged(__res)"
                : ReturnStatementValue(VariantTypeToCSharpType(t)));
            file.WriteLine(";");
            file.WriteLine("\t}");
        }
        file.WriteLine();

        for (var i = 1; i < types.Length; i++)
        {
            string t = types[i];
            file.Write("\tpublic ");
            file.Write(VariantTypeToCSharpType(t));
            file.Write(" As");
            file.Write(t);
            file.Write("() => Get");
            file.Write(t);
            file.WriteLine("FromVariant(this);");
        }
        file.WriteLine();

        for (var i = 1; i < types.Length; i++)
        {
            string t = types[i];
            file.Write("\tpublic static implicit operator Variant(");
            file.Write(VariantTypeToCSharpType(t));
            file.WriteLine(" value) => new Variant(value);");
        }
        file.WriteLine();

        for (var i = 1; i < types.Length; i++)
        {
            string t = types[i];
            file.Write("\tpublic static explicit operator ");
            file.Write(VariantTypeToCSharpType(t));
            file.Write("(Variant value) => value.As");
            file.Write(t);
            file.WriteLine("();");
        }
        file.WriteLine();
        file.WriteLine("}");
        file.Close();
    }
}
