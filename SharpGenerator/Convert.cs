using System.Xml.Serialization;
using Microsoft.VisualBasic;
using SharpGenerator.Documentation;

namespace SharpGenerator;

public class Convert
{
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
        ["builtin"] = new() { "StringName", "Variant" },
        ["utility"] = new(),
        ["core"] = new(),
        ["servers"] = new(),
        ["scene"] = new(),
        ["editor"] = new(),
    };

    private readonly Api api;
    private readonly XmlSerializer classXml = new(typeof(Documentation.Class));
    private readonly XmlSerializer builtinXml = new(typeof(Documentation.BuiltinClass));
    private readonly string csDir;
    private readonly string cppDir;
    private readonly string docDir;
    private readonly string configName;
    private static readonly Dictionary<string, int> BuiltinClassSizes = new();

    public Convert(Api api, string csDir, string cppDir, string docDir, string configName)
    {
        this.api = api;
        this.csDir = csDir;
        this.cppDir = cppDir;
        this.docDir = docDir;
        this.configName = configName;
        foreach (Api.BuiltinClassSizes classSizes in api.builtinClassSizes)
        {
            if (classSizes.buildConfiguration != configName) continue;
            foreach (Api.Size size in classSizes.sizes)
            {
                BuiltinClassSizes[size.name] = size.size;
            }
        }
    }

    public Dictionary<string, List<string>> BuiltinClassFunctions { get; } = new();
    public Dictionary<string, List<string>> ClassFunctions { get; } = new();

    public void Emit()
    {

        foreach (string o in builtinObjectTypes)
        {
            objectTypes.Add(o);
        }

        foreach (Api.Class c in api.classes)
        {
            objectTypes.Add(c.name);
        }

        BuiltinClasses();
        Classes();

        Directory.CreateDirectory(csDir + "/Enums");
        foreach (Api.Enum e in api.globalEnums)
        {
            GlobalEnum(e, csDir + "/Enums");
        }

        Directory.CreateDirectory(csDir + "/NativeStructures");
        foreach (Api.NativeStructure native in api.nativeStructures)
        {
            StreamWriter file = File.CreateText(csDir + "/NativeStructures/" + Fixer.CSType(native.name, api) + ".cs");
            file.WriteLine("namespace GodotSharpGDExtension;");
            file.WriteLine(value: "[StructLayout(LayoutKind.Sequential)]");
            file.WriteLine($"public unsafe struct {native.name} {{");
            foreach (string member in native.format.Split(";"))
            {
                string[] pair = member.Split(" ");
                string name = Fixer.Name(pair[1]);
                string type = Fixer.CSType(pair[0], api);
                if (name.Contains('*'))
                {
                    type = "IntPtr"; //pointer to `'Object', which is managed in bindings
                    name = name.Replace("*", "");
                }
                else if (builtinObjectTypes.Contains(type))
                {
                    type = "IntPtr";
                }
                if (name.Contains('['))
                {
                    int size = int.Parse(name.Split("[")[1].Split("]")[0]);
                    name = name.Split("[")[0];
                    for (var i = 0; i < size; i++)
                    {
                        file.WriteLine($"\t{type} {name}{i};");
                    }
                    continue;
                }
                file.WriteLine($"\t{type} {name};");
            }
            file.WriteLine("}");
            file.Close();
        }
        Directory.CreateDirectory(csDir + "/UtilityFunctions");
        Class docGlobalScope = GetDocs("@GlobalScope");
        var files = new Dictionary<string, (StreamWriter, List<string>)>();
        foreach (Api.Method f in api.untilityFunction)
        {
            string cat = string.Concat(f.category![0].ToString().ToUpper(), f.category.AsSpan(1));
            if (files.TryGetValue(cat, out (StreamWriter, List<string>) file) == false)
            {
                file = (File.CreateText(csDir + "/UtilityFunctions/" + cat + ".cs"), new List<string>());
                files.Add(cat, file);
                file.Item1.WriteLine("namespace GodotSharpGDExtension;");
                file.Item1.WriteLine($"public static unsafe partial class {cat} {{");
                registrations["utility"].Add(cat);
            }
            Documentation.Method d = null;
            if (docGlobalScope is { methods: not null })
            {
                d = docGlobalScope.methods.FirstOrDefault(x => x.name == f.name);
            }
            Method(f, "", file.Item1, MethodType.Utility, file.Item2, d);
        }
        foreach ((string _, (StreamWriter file, List<string> list)) in files)
        {
            for (var i = 0; i < list.Count; i++)
            {
                file.WriteLine($"\tstatic IntPtr __methodPointer{i} = {list[i]};");
            }
            file.WriteLine();

            file.WriteLine("\tpublic static void Register() {");
            file.WriteLine("\t}");
            file.WriteLine(value: "}");
            file.Close();
        }

        StreamWriter register = File.CreateText(csDir + "/Register.cs");
        register.WriteLine("namespace GodotSharpGDExtension;");
        register.WriteLine("public static class Register {");
        foreach ((string key, HashSet<string> list) in registrations)
        {
            register.WriteLine($"\tpublic static void Register{Fixer.SnakeToPascal(key)}() {{");
            foreach (string r in list)
            {
                register.WriteLine($"\t\t{r}.Register();");
            }
            register.WriteLine("\t}");
        }
        register.WriteLine("}");
        register.Close();

        Variant();
    }

    private Documentation.Class GetDocs(string name)
    {
        if (docDir == null) { return null; }
        string path = docDir + name + ".xml";
        if (File.Exists(path))
        {
            FileStream file = File.OpenRead(path);
            var d = (Documentation.Class)classXml.Deserialize(file)!;
            file.Close();
            return d;
        }
        else
        {
            return null;
        }
    }

    private Documentation.BuiltinClass GetBuiltinDocs(string name)
    {
        if (docDir == null) { return null; }
        string path = docDir + name + ".xml";
        if (File.Exists(path))
        {
            FileStream file = File.OpenRead(path);
            var d = (Documentation.BuiltinClass)builtinXml.Deserialize(file)!;
            file.Close();
            return d;
        }
        else
        {
            return null;
        }
    }

    private void GlobalEnum(Api.Enum e, string dir)
    {
        if (e.name.Contains('.')) { return; }
        string name = Fixer.CSType(e.name, api).Replace(".", "");
        StreamWriter file = File.CreateText(dir + "/" + Fixer.CSType(name, api) + ".cs");
        file.WriteLine("namespace GodotSharpGDExtension {");
        Enum(e, file);
        file.WriteLine("}");
        file.Close();
    }

    private void BuiltinClasses()
    {
        string csDir = this.csDir + "/BuiltinClasses";
        string cppDir = this.cppDir + "/BuiltinClasses";
        Directory.CreateDirectory(csDir);
        Directory.CreateDirectory(cppDir);
        foreach (Api.BuiltinClass c in api.builtinClasses)
        {
            switch (c.name)
            {
                case "int":
                case "float":
                case "bool":
                case "String":
                case "Nil":
                    break;
                default:
                    BuiltinClass(c, csDir, cppDir, builtinObjectTypes.Contains(c.name));
                    break;
            }
        }
    }

    private void BuiltinClass(Api.BuiltinClass c, string csDir, string cppDir, bool hasPointer)
    {
        string className = c.name;
        if (className == "Object") className = "GodotObject";
        StreamWriter csFile = File.CreateText(csDir + "/" + Fixer.CSType(className, api) + ".cs");
        StreamWriter cppHeaderFile = File.CreateText(cppDir + "/" + Fixer.CSType(className, api) + ".hpp");
        StreamWriter cppSourceFile = File.CreateText(cppDir + "/" + Fixer.CSType(className, api) + ".cpp");
        registrations["builtin"].Add(Fixer.CSType(className, api));

        var classFunctions = new List<string>();
        BuiltinClassFunctions[className] = classFunctions;
        
        BuiltinClass doc = GetBuiltinDocs(className);

        var constructorRegistrations = new List<string>();
        var operatorRegistrations = new List<string>();
        var methodRegistrations = new List<string>();

        int size = BuiltinClassSizes[c.name];
        
        foreach (Api.BuiltinClassSizes config in api.builtinClassSizes)
        {
            if (config.buildConfiguration != configName)
            {
                continue;
            }

            foreach (Api.Size sizePair in config.sizes)
            {
                if (sizePair.name != className)
                {
                    continue;
                }

                break;
            }
            break;
        }

        cppHeaderFile.WriteLine("#pragma once");
        cppHeaderFile.WriteLine();
        cppHeaderFile.WriteLine("#include \"gdextension_interface.h\"");
        cppHeaderFile.WriteLine("#include \"godot_cpp/core/defs.hpp\"");
        cppHeaderFile.WriteLine();
        
        cppSourceFile.WriteLine($"#include \"{Fixer.CSType(className, api)}.hpp\"");
        cppSourceFile.WriteLine();
        cppSourceFile.WriteLine("#include <array>");
        cppSourceFile.WriteLine("#include \"godot_cpp/variant/string_name.hpp\"");
        cppSourceFile.WriteLine("#include \"godot_cpp/godot.hpp\"");
        
        // TODO: remove?
        // if (hasPointer == false)
        // {
        //     csFile.WriteLine($"[StructLayout(LayoutKind.Explicit, Size = {size})]");
        // }
        cppHeaderFile.WriteLine("namespace GodotSharpGDExtension {");
        csFile.WriteLine("namespace GodotSharpGDExtension;");
        csFile.WriteLine();
        csFile.WriteLine($"public unsafe class {Fixer.CSType(className, api)} {{");    
        // TODO: remove?
        // csFile.WriteLine("\tstatic private bool _registered = false;");
        // csFile.WriteLine();

        // TODO: remove?
        // csFile.WriteLine($"\tpublic const int StructSize = {size};");
        csFile.WriteLine("\tpublic IntPtr InternalPointer { get; }");
        csFile.WriteLine($"\tpublic {className}(IntPtr ptr) => InternalPointer = ptr;");
        csFile.WriteLine();

        if (c.isKeyed)
        {
            //Dictionary
            //todo: manually as extension?
        }

        if (c.indexingReturnType != null)
        {
            //array?
            //todo: manually as extension?
        }

        var membersWithFunctions = new List<string>();

        if (c.members != null)
        {
            foreach (Api.Member member in c.members)
            {
                Member? d = null;
                if (doc != null && doc.members != null)
                {
                    d = doc.members.FirstOrDefault(x => x.name == member.name);
                }
                Member(member, csFile, cppHeaderFile, cppSourceFile, className, d, classFunctions);
            }
        }
        
        if (c.constants != null)
        {
            foreach (Api.Constant con in c.constants)
            {
                if (doc is { constants: not null })
                {
                    Constant d = doc.constants.FirstOrDefault(x => x.name == con.name);
                    if (d != null && d.comment != null)
                    {
                        string com = Fixer.XMLComment(d.comment);
                        csFile.WriteLine(com);
                    }
                }
                csFile.WriteLine($"\tpublic static {Fixer.CSType(con.type, api)} {Fixer.SnakeToPascal(con.name)} => {Fixer.Value(con.value)};");
            }
            csFile.WriteLine();
        }

        if (c.constructors != null)
        {
            for (var i = 0; i < c.constructors.Length; i++)
            {
                Api.Constructor constructor = c.constructors[i];
                Constructor? d = null;
                if (doc != null && doc.constructors != null)
                {
                    d = doc.constructors[i];
                }
                Constructor(c, constructor, csFile, cppHeaderFile, cppSourceFile, constructorRegistrations, d, hasPointer, size, classFunctions);
            }
        }
        else
        {
            var emptyApiConstructor = new Api.Constructor { arguments = Array.Empty<Api.Argument>(), index = 0};
            var emptyDocConstructor = new Constructor();
            Constructor(c, emptyApiConstructor, csFile, cppHeaderFile, cppSourceFile, constructorRegistrations, emptyDocConstructor, hasPointer, size, classFunctions);
        }

        if (c.operators != null)
        {
            foreach (Api.Operator op in c.operators)
            {
                Operator? d = null;
                if (doc != null && doc.operators != null)
                {
                    d = doc.operators.FirstOrDefault(x => x.name == $"operator {op.name}");
                }
                Operator(op, className, csFile, operatorRegistrations, d);
            }
        }

        if (c.enums != null)
        {
            foreach (Api.Enum e in c.enums)
            {
                Enum(e, csFile, doc?.constants);
            }
        }

        if (c.methods != null)
        {
            foreach (Api.Method meth in c.methods)
            {
                Documentation.Method d = null;
                if (doc != null && doc.methods != null)
                {
                    d = doc.methods.FirstOrDefault(x => x.name == meth.name);
                }
                Method(meth, className, csFile, MethodType.Native, methodRegistrations, d, isBuiltinPointer: hasPointer);
            }
        }

        EqualAndHash(className, csFile);

        if (hasPointer)
        {
            csFile.WriteLine($"\t~{Fixer.CSType(className, api)}() {{");
            //file.WriteLine($"\t\tif(internalPointer == null) {{ return; }}");
            csFile.WriteLine($"\t\tGDExtensionInterface.CallGDExtensionPtrDestructor(__destructor, internalPointer);");
            //file.WriteLine($"\t\tGDExtensionInterface.MemFree(internalPointer);");
            //file.WriteLine($"\t\tinternalPointer = null;");
            csFile.WriteLine($"\t}}");
            csFile.WriteLine();
            csFile.WriteLine("\t[StructLayout(LayoutKind.Explicit, Size = StructSize)]");
            csFile.WriteLine("\tpublic struct InternalStruct { }");
            csFile.WriteLine();
        }


        for (var i = 0; i < constructorRegistrations.Count; i++)
        {
            csFile.WriteLine($"\tstatic IntPtr __constructorPointer{i} => {constructorRegistrations[i]};");
        }
        for (var i = 0; i < operatorRegistrations.Count; i++)
        {
            csFile.WriteLine($"\tstatic IntPtr __operatorPointer{i} => {operatorRegistrations[i]};");
        }
        for (var i = 0; i < methodRegistrations.Count; i++)
        {
            csFile.WriteLine($"\tstatic IntPtr __methodPointer{i} => {methodRegistrations[i]};");
        }

        if (hasPointer)
        {
            csFile.WriteLine($"\tstatic IntPtr __destructor => GDExtensionInterface.VariantGetPtrDestructor((GDExtensionVariantType)Variant.Type.{Fixer.CSType(className, api)});");
        }

        csFile.WriteLine();

        csFile.WriteLine("\tpublic static void Register() {");
        csFile.WriteLine("\t\tif (_registered) return;");
        csFile.WriteLine("\t\t_registered = true;");
        if (hasPointer)
        {
            // file.WriteLine($"\t\t__destructor = GDExtensionInterface.VariantGetPtrDestructor((GDExtensionVariantType)Variant.Type.{Fixer.Type(className, api)});");
        }
        foreach (string member in membersWithFunctions)
        {
            // file.WriteLine($"\t\tvar _stringName{member} = new StringName(\"{member}\");");
            // file.WriteLine($"\t\t{member}Getter = GDExtensionInterface.VariantGetPtrGetter((GDExtensionVariantType)Variant.Type.{Fixer.Type(className, api)}, _stringName{member}.internalPointer);");
            // file.WriteLine($"\t\t{member}Setter = GDExtensionInterface.VariantGetPtrSetter((GDExtensionVariantType)Variant.Type.{Fixer.Type(className, api)}, _stringName{member}.internalPointer);");
        }
        for (var i = 0; i < operatorRegistrations.Count; i++)
        {
            // file.WriteLine(operatorRegistrations[i]);
        }
        // if (c.constants != null)
        // {
        //     foreach (var con in c.constants)
        //     {
        //         file.WriteLine($"\t\t{Fixer.SnakeToPascal(con.name)} = {Fixer.Value(con.value)};");
        //     }
        // }
        csFile.WriteLine("\t}");
        csFile.WriteLine($" }} { (hasPointer ? "" : $"{Fixer.CSType(className, api)};")}");
        csFile.Close();
        cppHeaderFile.WriteLine("}");
        
        cppHeaderFile.Close();
        cppSourceFile.Close();
    }

    private void Member(Api.Member member, TextWriter csFile,
        TextWriter cppHeaderFile, TextWriter cppSourceFile, string className, Member? doc, ICollection<string> classFunctions)
    {
        if (doc != null)
        {
            string com = Fixer.XMLComment(doc.comment);
            csFile.WriteLine(com);
        }

        var getterName = $"{className}_{member.name}_getter";
        var setterName = $"{className}_{member.name}_setter";

        string cppType = Fixer.CPPType(member.type, api);
        bool isPod = Fixer.IsPod(member.type);
        
        (string? memberType, string? returnText) = isPod ? (null, null) : Fixer.GetReturnDataForType(cppType);

        
        
        memberType ??= cppType;
        returnText ??= "return {0};";
        
        
        var getterSignature = $"{memberType} {getterName}(GDExtensionTypePtr p_base)";
        var setterSignature = $"void {setterName}(GDExtensionTypePtr p_base, {memberType} p_value)";
        var getterCall = $"GDExtensionInterface.{Fixer.SnakeToPascal(getterName)}(InternalPointer)";
        csFile.WriteLine($$"""
                           	public {{cppType}} {{Fixer.SnakeToPascal(member.name)}}
                           	{
                           		get => {{(isPod ? getterCall : $"new({getterCall})")}};
                           		set => GDExtensionInterface.{{Fixer.SnakeToPascal(setterName)}}(InternalPointer, {{(isPod ? "value" : "value.InternalPointer")}});
                           	}
                           """);
        cppHeaderFile.WriteLine($"""
                                 GDE_EXPORT {getterSignature};
                                 GDE_EXPORT {setterSignature};
                                 """);


        // foreach (string member in membersWithFunctions)
        // {
        //     csFile.WriteLine($"\tprivate static StringName _stringName{member} => *(StringName*)GDExtensionInterface.CreateStringName (\"{member}\");");
        //     csFile.WriteLine($"\tstatic IntPtr {member}Getter => GDExtensionInterface.VariantGetPtrGetter((GDExtensionVariantType)Variant.Type.{Fixer.Type(className, api)}, _stringName{member}.internalPointer);");
        //     csFile.WriteLine($"\tstatic IntPtr {member}Setter => GDExtensionInterface.VariantGetPtrSetter((GDExtensionVariantType)Variant.Type.{Fixer.Type(className, api)}, _stringName{member}.internalPointer);");
        // }            

        cppSourceFile.WriteLine();
        cppSourceFile.WriteLine($"{getterSignature} {{");
        cppSourceFile.WriteLine($"static auto func = godot::internal::gdextension_interface_variant_get_ptr_getter(GDEXTENSION_VARIANT_TYPE_{cppType.ToUpper()}, godot::StringName(\"{member.name}\")._native_ptr());");
        cppSourceFile.WriteLine(
            isPod 
                ? $"{memberType} value;"
                : $"auto value = godot::internal::gdextension_interface_mem_alloc({BuiltinClassSizes[member.type]});");
        cppSourceFile.WriteLine(
            isPod
            ? "func(p_base, &value);"
            : "func(p_base, value);");
        cppSourceFile.WriteLine(returnText, "value");
        cppSourceFile.WriteLine("}");

        classFunctions.Add(getterName);
        classFunctions.Add(setterName);
        csFile.WriteLine();
    }

    private void Constructor(Api.BuiltinClass c, Api.Constructor constructor, TextWriter csFile, TextWriter cppHeaderFile,
        TextWriter cppSourceFile, ICollection<string> constructorRegistrations, Constructor? doc, bool hasPointer,
        int size, ICollection<string> classFunctions)
    {
        if (doc != null)
        {
            string com = Fixer.XMLComment(doc.description);
            csFile.WriteLine(com);
        }
        csFile.Write($"\tpublic {Fixer.CSType(c.name, api)}(");
        var csArgs = new List<string>();
        var nativeArgs = new List<string>();
        var csArgPAsses = new List<string>();
        var nativeArgPasses = new List<string>();

        
        if (constructor.arguments != null)
        {
            foreach (Api.Argument arg in constructor.arguments)
            {
                bool isPod = Fixer.IsPod(arg.type);
                csArgs.Add($"{Fixer.CSType(arg.type, api)} {Fixer.Name(arg.name)}");
                nativeArgs.Add($"{(isPod ? Fixer.CPPType(arg.type, api) : "GDExtensionTypePtr")} {arg.name}");
                csArgPAsses.Add(isPod ? arg.name : $"{arg.name}.InternalPointer");
                nativeArgPasses.Add(isPod ? $"&{arg.name}" : arg.name);
            }
        }
        const string argSeparator = ", ";

        var nativeFunctionName = $"{c.name}_constructor_{constructor.index}";
        
        var functionSignature = $"GDExtensionTypePtr {nativeFunctionName}({string.Join(argSeparator, nativeArgs)})";
        
        cppHeaderFile.WriteLine($"GDE_EXPORT {functionSignature};");
        cppHeaderFile.WriteLine();
        
        
        // static auto constructor = godot::internal::gdextension_interface_variant_get_ptr_constructor(GDEXTENSION_VARIANT_TYPE_AABB, 1);
        // auto base = new uint8_t[8];
        // std::array<GDExtensionConstTypePtr, 1> call_args = {from};
        // constructor(base, call_args.data());
        // return base;

        cppSourceFile.WriteLine($"{functionSignature} {{");
        cppSourceFile.WriteLine($"\tstatic auto constructor = godot::internal::gdextension_interface_variant_get_ptr_constructor(GDEXTENSION_VARIANT_TYPE_{c.name.ToUpper()}, {constructor.index});");
        cppSourceFile.WriteLine($"\tauto base = new uint8_t[{size}];");
        if (nativeArgPasses.Any())
        {
            cppSourceFile.WriteLine($"\tstd::array<GDExtensionConstTypePtr, {nativeArgPasses.Count}> call_args = {{{string.Join(argSeparator, nativeArgPasses)}}};");
        }
        cppSourceFile.Write("\tconstructor(base");
        cppSourceFile.WriteLine(nativeArgPasses.Any() ? ", call_args.data());" : ", nullptr);");
        cppSourceFile.WriteLine("\treturn base;");
        cppSourceFile.WriteLine("}");
        cppSourceFile.WriteLine();
        
        csFile.Write(string.Join(argSeparator, csArgs));
        csFile.WriteLine(") {");
        csFile.Write($"\t\tInternalPointer = GDExtensionInterface.{Fixer.SnakeToPascal(nativeFunctionName)}(");
        csFile.Write(string.Join(argSeparator, csArgPAsses));
        csFile.WriteLine(");");
        csFile.WriteLine("\t}");
        
        
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

    private void Operator(Api.Operator op, string className, StreamWriter file, List<string> operatorRegistrations, Operator? doc)
    {
        // TODO
        if (op.rightType != null)
        {
            if (op.rightType == "Variant") { return; }
            string name = op.name switch
            {
                "or" => "operator |",
                "and" => "operator &",
                "xor" => "operator ^",
                "**" => "OperatorPower",
                "in" => "OperatorIn",
                _ => $"operator {op.name}",
            };
            if (doc != null)
            {
                file.WriteLine(Fixer.XMLComment(doc.description));
            }
            file.WriteLine($"\tpublic static {Fixer.CSType(op.returnType, api)} {name}({Fixer.CSType(className, api)} left, {Fixer.CSType(op.rightType, api)} right) {{");
            var m = $"__operatorPointer{operatorRegistrations.Count}";
            file.WriteLine($"\t\tvar __op = {m};");
            operatorRegistrations.Add($"GDExtensionInterface.VariantGetPtrOperatorEvaluator((GDExtensionVariantOperator)Variant.Operator.{Fixer.VariantOperator(op.name)}, (GDExtensionVariantType)Variant.Type.{className}, (GDExtensionVariantType)Variant.Type.{Fixer.VariantName(op.rightType)})");
            file.WriteLine($"\t\tIntPtr __res = GDExtensionInterface.CallGDExtensionPtrOperatorEvaluator(__op, {ValueToPointer("left", className)}, {ValueToPointer("right", op.rightType)});");
            file.WriteLine($"\t\treturn {ReturnStatementValue(op.returnType)};");
        }
        else
        {
            string name = op.name switch
            {
                "unary-" => "operator -",
                "not" => "operator !",
                "unary+" => "operator +",
                _ => $"operator {op.name}",
            };
            if (doc != null)
            {
                file.WriteLine(Fixer.XMLComment(doc.description));
            }
            file.WriteLine($"\tpublic static {Fixer.CSType(op.returnType, api)} {name}({Fixer.CSType(className, api)} value) {{");
            var m = $"__operatorPointer{operatorRegistrations.Count}";
            file.WriteLine($"\t\tvar __op = {m};");
            operatorRegistrations.Add($"GDExtensionInterface.VariantGetPtrOperatorEvaluator((GDExtensionVariantOperator)Variant.Operator.{Fixer.VariantOperator(op.name)}, (GDExtensionVariantType)Variant.Type.{className}, (GDExtensionVariantType)Variant.Type.Nil)");
            file.WriteLine($"\t\tIntPtr __res = GDExtensionInterface.CallGDExtensionPtrOperatorEvaluator(__op, {ValueToPointer("value", className)}, IntPtr.Zero);");
            file.WriteLine($"\t\treturn {ReturnStatementValue(op.returnType)};");
        }
        file.WriteLine("\t}");
        file.WriteLine();
    }

    private void Enum(Api.Enum e, StreamWriter file, Constant[]? constants = null)
    {
        // TODO
        int prefixLength = Fixer.SharedPrefixLength(e.values.Select(x => x.name).ToArray());
        if (e.isBitfield ?? false)
        {
            file.WriteLine($"\t[Flags]");
        }

        file.WriteLine($"\tpublic enum {Fixer.CSType(e.name, api)} {{");
        foreach (Api.Value v in e.values)
        {
            if (constants != null)
            {
                Constant d = constants.FirstOrDefault(x => x.@enum != null && x.@enum == e.name && x.name == v.name);
                if (d != null && d.comment != null)
                {
                    file.WriteLine(Fixer.XMLComment(d.comment, 2));
                }
            }
            string name = Fixer.SnakeToPascal(v.name[prefixLength..]);
            if (char.IsDigit(name[0])) { name = "_" + name; }
            file.WriteLine($"\t\t{name} = {v.value},");
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
        else if (objectTypes.Contains(type) || builtinObjectTypes.Contains(f))
        {
            return $"{name}.internalPointer";
        }
        else
        {
            return $"(IntPtr)(&{Fixer.Name(name)})";
        }
    }

    private bool IsClassType(string type)
    {
        string f = Fixer.CSType(type, api);
        return f == "Array" || f.StartsWith("Array<") || f == "Variant";
    }

    private string ReturnLocationType(string type, string name)
    {
        string f = Fixer.CSType(type, api);
        if(f == "Variant")
        {
            return $"var {name} = new Variant()";
        }
        if (IsClassType(type))
        {
            return $"{f} {name} = new {f}();";
        }
        if (builtinObjectTypes.Contains(f))
        {
            return $"{f}.InternalStruct {name}";
        }
        else if (objectTypes.Contains(type) || type == "String")
        {
            return $"IntPtr {name}";
        }
        else
        {
            return $"{f} {name}";
        }
    }

    private string ReturnStatementValue(string type)
    {
        string f = Fixer.CSType(type, api);
        if (type == "String")
        {
            return "StringMarshall.ToManaged(__res)";
        }
        else if (builtinObjectTypes.Contains(f))
        {
            if (f == "Array")
            {
                return $"new {f}(GDExtensionMain.MoveToUnmanaged(__res))";
            }
            return $"new {f}(__res)";
        }
        else if(f == "Variant")
        {
            return $"__res";
        }
        else if (f == "GodotObject")
        {
            return "GodotObject.ConstructUnknown(__res)";
        }
        else if (objectTypes.Contains(type))
        {
            return $"({f})GodotObject.ConstructUnknown(__res)";
        }
        else
        {
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
                    };
                }
            }
            
            return $"{castText}{marshalText}";
        }
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
        if (value == "{}") { return false; }
        if (value == "[]") { return false; }
        if (value.Contains('&')) { return false; }
        if (value == "") { return type == "String"; }
        if (type == "Variant") { return false; }
        if (type == "StringName") { return false; }
        return true;
    }

    private string FixDefaultValue(string value, string type)
    {
        if (value.StartsWith("Array["))
        {
            return $"new()";
        }
        if (value.Contains('(')) { return $"new {value}"; }
        if (value == "{}") { return "new Dictionary()"; }
        if (value == "[]") { return "new()"; }
        if (value.Contains('&')) { return $"new StringName({value[1..]})"; }
        if (value == "") { return $"new {type}()"; }
        if (type == "Variant" && value == "null") { return "Variant.Nil"; }
        if (value == "null") { return "null"; }
        return $"({Fixer.CSType(type, api)}){value}";
    }

    private void Method(Api.Method meth, string className, StreamWriter file, MethodType type, List<string> methodRegistrations, Documentation.Method doc, bool isSingleton = false, bool isBuiltinPointer = false)
    {
        // TODO
        var header = "";
        if (doc != null)
        {
            if (doc.description != null)
            {
                header += Fixer.XMLComment(doc.description) + Environment.NewLine;
            }
        }
        header += "\tpublic ";
        string ret = meth.returnType ?? meth.returnValue?.type ?? "";
        if ((meth.isStatic ?? false) || type == MethodType.Utility || isSingleton)
        {
            header += "static ";
        }
        if (meth.isVirtual)
        {
            header += "virtual ";
        }
        if (meth.name == "to_string")
        {
            header += "new ";
        }

        if (!string.IsNullOrEmpty(ret))
        {
            header += Fixer.CSType(ret, api);
        }
        else
        {
            header += "void";
        }
        header += $" {Fixer.MethodName(meth.name)}(";
        if (meth.arguments != null)
        {
            for (var i = 0; i < meth.arguments.Length; i++)
            {
                Api.Argument arg = meth.arguments[i];
                var suffix = "";
                if (arg.defaultValue != null)
                {
                    var validDefault = true;
                    for (int j = i; j < meth.arguments.Length; j++)
                    {
                        validDefault &= IsValidDefaultValue(meth.arguments[j].defaultValue!, meth.arguments[j].type);
                    }
                    if (validDefault)
                    {
                        suffix = $" = {FixDefaultValue(arg.defaultValue, arg.type)}";
                    }
                    else
                    {
                        file.Write(header + $") => {Fixer.MethodName(meth.name)}(");
                        for (var j = 0; j < i; j++)
                        {
                            file.Write($"{Fixer.Name(meth.arguments[j].name)}, ");
                        }
                        for (int j = i; j < meth.arguments.Length; j++)
                        {
                            file.Write($"{FixDefaultValue(meth.arguments[j].defaultValue!, meth.arguments[j].type)}");
                            if (j < meth.arguments.Length - 1)
                            {
                                file.Write(", ");
                            }
                        }
                        file.WriteLine(");");
                    }
                }
                if (i != 0)
                {
                    header += ", ";
                }
                header += $"{Fixer.CSType(arg.type, api)} {Fixer.Name(arg.name)}{suffix}";
            }
        }

        file.Write(header);
        if (meth.isVararg)
        {
            if (meth.arguments != null)
            {
                file.Write(", ");
            }
            file.Write("params Variant[] arguments");
        }
        file.WriteLine(") {");
        if ((meth.isStatic ?? false) || type == MethodType.Utility || isSingleton)
        {
            file.WriteLine("\t\tRegister();");
        }
        
        var m = "";
        switch (type)
        {
            case MethodType.Class:
                if (meth.isVirtual || meth.hash is null)
                {
                    if (!string.IsNullOrEmpty(ret))
                    {
                        file.WriteLine($"return default;");
                    }
                    file.WriteLine("\t}");
                    return;
                }
                m = $"__methodPointer{methodRegistrations.Count}";
                methodRegistrations.Add($"GDExtensionInterface.ClassdbGetMethodBind(__godot_name.internalPointer, new StringName(\"{meth.name}\").internalPointer, {meth.hash ?? 0})");
                break;
            case MethodType.Native:
                m = $"__methodPointer{methodRegistrations.Count}";
                methodRegistrations.Add($"GDExtensionInterface.VariantGetPtrBuiltinMethod((GDExtensionVariantType)Variant.Type.{className}, new StringName(\"{meth.name}\").internalPointer, {meth.hash ?? 0})");
                break;
            case MethodType.Utility:
                m = $"__methodPointer{methodRegistrations.Count}";
                methodRegistrations.Add($"GDExtensionInterface.VariantGetPtrUtilityFunction(new StringName(\"{meth.name}\").internalPointer, {meth.hash ?? 0})");
                break;
        }
        if (meth.isVararg)
        {
            string t;
            if (type == MethodType.Class)
            {
                t = "IntPtr";
            }
            else
            {
                t = "IntPtr";
            }
            if (meth.arguments != null)
            {
                file.WriteLine($"\t\tvar __args = stackalloc {t}[{meth.arguments.Length} + arguments.Length];");
            }
            else
            {
                file.WriteLine($"\t\tvar __args = stackalloc {t}[arguments.Length];");
            }
        }
        else if (meth.arguments != null)
        {
            file.WriteLine($"\t\tvar __args = stackalloc IntPtr[{meth.arguments.Length}];");
        }
        if (meth.arguments != null)
        {
            for (var i = 0; i < meth.arguments.Length; i++)
            {
                Api.Argument arg = meth.arguments[i];
                file.Write($"\t\t__args[{i}] = ");
                if (meth.isVararg)
                {
                    string val = arg.type != "Variant" ? $"new Variant({Fixer.Name(arg.name)})" : Fixer.Name(arg.name);
                    file.WriteLine($"{val}.internalPointer;");
                }
                else
                {
                    file.WriteLine($"{ValueToPointer(Fixer.Name(arg.name), arg.type)};");
                }
            }
        }
        if (meth.isVararg)
        {
            string offset = meth.arguments != null ? $"{meth.arguments.Length} + " : "";
            file.WriteLine($"\t\tfor (var i = 0; i < arguments.Length; i++) {{");
            file.WriteLine($"\t\t\t__args[{offset}i] = arguments[i].internalPointer;");
            file.WriteLine("\t\t};");
        }
        if (meth.isStatic == false && type == MethodType.Native && isBuiltinPointer == false)
        {
            file.WriteLine($"\t\tvar __temp = this;");
        }
        if (type == MethodType.Class)
        {

            string call;
            if (meth.isVararg)
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

            file.Write(call);
        }
        else if (type == MethodType.Utility)
        {
            file.Write($"\t\tIntPtr __res =GDExtensionInterface.CallGDExtensionPtrUtilityFunction({m}");
        }
        else
        {
            file.Write($"\t\tIntPtr __res = GDExtensionInterface.CallGDExtensionPtrBuiltInMethod({m}, ");
        }

        if (type != MethodType.Utility)
        {
            if (meth.isStatic ?? false)
            {
                file.Write("IntPtr.Zero");
            }
            else if (type == MethodType.Class)
            {
                file.Write(value: $"{(isSingleton ? "Singleton" : "this")}.internalPointer");
            }
            else if (isBuiltinPointer)
            {
                file.Write("this.internalPointer");
            }
            else
            {
                file.Write("(IntPtr)(&__temp)");
            }
        }

        if (meth.arguments != null || meth.isVararg)
        {
            file.Write(", *__args");
        }
        else
        {
            file.Write(", IntPtr.Zero");
        }
        if (type == MethodType.Class && meth.isVararg)
        {
            file.Write($", {(meth.arguments != null ? $"{meth.arguments.Length} + " : "")}arguments.Length");
        }

        if (type != MethodType.Class)
        {
            file.Write(", ");
            if (meth.isVararg)
            {
                file.Write($"{(meth.arguments != null ? $"{meth.arguments.Length} + " : "")}arguments.Length");
            }
            else if (meth.arguments != null)
            {
                file.Write($"{meth.arguments.Length}");
            }
            else
            {
                file.Write("0");
            }
        }
        if (type == MethodType.Class && meth.isVararg)
        {
            file.Write(", out IntPtr __res");
            // file.Write(", out GDExtensionCallError __err");
            file.Write(", out GDExtensionCallError _");
            
        }
        file.WriteLine(");");
        if (ret != "")
        {
            file.WriteLine($"\t\treturn {ReturnStatementValue(ret)};");
        }
        file.WriteLine("\t}");
        file.WriteLine();
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
        foreach (Api.Class c in api.classes)
            if (cName == c.name)
            {
                if (c.methods != null)
                {
                    foreach (Api.Method m in c.methods!)
                    {
                        if (m.name == name)
                        {
                            return m;
                        }
                    }
                }

                string inherits = c.inherits;
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
        string dir = this.csDir + "/Classes";
        Directory.CreateDirectory(dir);
        foreach (Api.Class c in api.classes)
        {
            switch (c.name)
            {
                case "int":
                case "float":
                case "bool":
                    break;
                default:
                    Class(c, dir);
                    break;
            }
        }
    }

    private void Class(Api.Class c, string dir)
    {
        // TODO
        string className = c.name;
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
            default:
                break;
        }

        StreamWriter file = File.CreateText(dir + "/" + className + ".cs");
        registrations[c.apiType].Add(className);

        Class doc = GetDocs(className);

        var methodRegistrations = new List<string>();

        file.WriteLine("namespace GodotSharpGDExtension;");
        file.WriteLine();
        file.Write("public unsafe ");
        bool isSingleton = api.singletons.Any(x => x.type == className);
        string inherits = c.inherits ?? "Wrapped";
        if (inherits == "Object")
        {
            inherits = "GodotObject";
        }
        file.WriteLine($"partial class {className} : {inherits} {{");
        file.WriteLine();
        file.WriteLine("\tprivate static bool _registered = false;");
        file.WriteLine();


        if (isSingleton)
        {
            file.WriteLine($"\tprivate static {className} _singleton = null;");
            file.WriteLine($"\tpublic static {className} Singleton {{");
            file.WriteLine($"\t\tget => _singleton ??= new {className}(GDExtensionInterface.GlobalGetSingleton(__godot_name.internalPointer));");
            file.WriteLine("\t}");
            file.WriteLine();
        }

        if (c.constants != null)
        {
            foreach (Api.Value con in c.constants)
            {
                if (doc != null)
                {
                    Constant d = doc.constants.FirstOrDefault(x => x.name == con.name);
                    if (d != null && d.comment != null)
                    {
                        string com = Fixer.XMLComment(d.comment);
                        file.WriteLine(com);
                    }
                }
                if (con.name.StartsWith("NOTIFICATION_"))
                {
                    file.WriteLine($"\tpublic const Notification {Fixer.SnakeToPascal(con.name)} = (Notification){con.value};");
                }
                else
                {
                    file.WriteLine($"\tpublic const int {con.name} = {con.value};");
                }
            }
            file.WriteLine();
        }

        if (c.enums != null)
        {
            foreach (Api.Enum e in c.enums)
            {
                Enum(e, file, doc?.constants);
            }
        }

        var addedMethods = new List<Api.Method>();

        if (c.properties != null)
        {
            foreach (Api.Property prop in c.properties)
            {
                string type = prop.type;
                var cast = "";

                Api.Method? getter = GetMethod(className, prop.getter);
                Api.Method? Setter = GetMethod(className, prop.setter);

                if (getter == null && Setter == null)
                {
                    var valType = new Api.ReturnValue
                    {
                        meta = type,
                        type = type,
                    };
                    if (!string.IsNullOrEmpty(prop.getter))
                    {
                        getter = new Api.Method
                        {
                            arguments = null,
                            category = null,
                            hash = null,
                            name = prop.getter,
                            returnType = type,
                            returnValue = valType,
                            isStatic = false,
                        };
                        addedMethods.Add(getter.Value);
                    }
                    if (!string.IsNullOrEmpty(prop.setter))
                    {
                        Setter = new Api.Method
                        {
                            arguments = new Api.Argument[1]
                            {
                            new Api.Argument
                            {
                                defaultValue = null,
                                name = "value",
                                type = type,
                                meta = type,
                            },
                            },
                            category = null,
                            hash = null,
                            name = prop.setter,
                            returnType = type,
                            returnValue = valType,
                            isStatic = false,
                        };
                        addedMethods.Add(Setter.Value);
                    }
                }
                if (doc != null && doc.members != null)
                {
                    Member d = doc.members.FirstOrDefault(x => x.name == prop.name);
                    if (d != null && d.comment != null)
                    {
                        string com = Fixer.XMLComment(d.comment);
                        file.WriteLine(com);
                    }
                }
                if (getter != null) { type = getter.Value.returnValue!.Value.type; } else { type = Setter!.Value.arguments![0].type; }

                bool hasEnumOfSameName = (c.enums?.Where(x => x.name == Fixer.Name(Fixer.SnakeToPascal(prop.name))).FirstOrDefault())?.name != null;

                file.Write($"\tpublic {Fixer.CSType(type, api)} {Fixer.Name(Fixer.SnakeToPascal(prop.name)) + (hasEnumOfSameName ? "Value" : "")} {{ ");

                if (prop.index.HasValue)
                {
                    if (getter == null)
                    {
                        throw new NotImplementedException("get cast from Setter");
                    }
                    cast = $"({Fixer.CSType(getter.Value.arguments![0].type, api)})";
                }

                if (getter != null)
                {
                    file.Write($"get => {Fixer.MethodName(prop.getter)}(");
                    if (prop.index.HasValue)
                    {
                        file.Write($"{cast}{prop.index.Value}");
                    }
                    file.Write("); ");
                }

                if (Setter != null)
                {
                    file.Write($"set => {Fixer.MethodName(prop.setter)}(");
                    if (prop.index.HasValue)
                    {
                        file.Write($"{cast}{prop.index.Value}, ");
                    }
                    file.Write("value); ");
                }
                file.WriteLine("}");
            }
            file.WriteLine();
        }


        addedMethods.AddRange(c.methods ?? Array.Empty<Api.Method>());

        foreach (Api.Method meth in addedMethods)
        {
            Documentation.Method d = null;
            if (doc != null && doc.methods != null)
            {
                d = doc.methods.FirstOrDefault(x => x.name == meth.name);
            }
            Method(meth, className, file, MethodType.Class, methodRegistrations, d, isSingleton: isSingleton);
        }
        if (c.signals != null)
        {
            foreach (Api.Signal sig in c.signals)
            {
                file.Write($"\tpublic void EmitSignal{Fixer.SnakeToPascal(sig.name)}(");
                if (sig.arguments != null)
                {
                    for (var j = 0; j < sig.arguments.Length; j++)
                    {
                        Api.Argument p = sig.arguments[j];
                        file.Write($"{Fixer.CSType(p.type, api)} {Fixer.Name(p.name)}{(j < sig.arguments.Length - 1 ? ", " : "")}");
                    }
                }

                file.Write($") => EmitSignal(\"{sig.name}\"{(sig.arguments != null ? ", " : "")}");
                if (sig.arguments != null)
                {
                    for (var j = 0; j < sig.arguments.Length; j++)
                    {
                        Api.Argument p = sig.arguments[j];
                        file.Write($"{Fixer.Name(p.name)}{(j < sig.arguments.Length - 1 ? ", " : "")}");
                    }
                }
                file.WriteLine(");");
                file.WriteLine();
                file.Write($"\tpublic delegate void Signal{Fixer.SnakeToPascal(sig.name)}(");
                if (sig.arguments != null)
                {
                    for (var j = 0; j < sig.arguments.Length; j++)
                    {
                        Api.Argument p = sig.arguments[j];
                        file.Write($"{Fixer.CSType(p.type, api)} {Fixer.Name(p.name)}{(j < sig.arguments.Length - 1 ? ", " : "")}");
                    }
                }
                file.WriteLine(");");
                file.WriteLine();
                file.WriteLine($"\tpublic event Signal{Fixer.SnakeToPascal(sig.name)} {Fixer.SnakeToPascal(sig.name)}{{");
                file.WriteLine($"\t\tadd => Connect(\"{sig.name}\", Callable.From(value, this));");
                file.WriteLine($"\t\tremove => Disconnect(\"{sig.name}\", Callable.From(value, this));");
                file.WriteLine("}");
                file.WriteLine();
            }
            file.WriteLine();
        }

        EqualAndHash(className, file);

        string content = className == "RefCounted" ? " Reference();\n" : "";

        content += "\tRegister();";

        file.WriteLine($"\tpublic {className}() : base(__godot_name) {{");
        file.WriteLine(content);
        file.WriteLine("}");
        file.WriteLine($"\tprotected {className}(StringName type) : base(type) {{{content}}}");
        file.WriteLine($"\tprotected {className}(IntPtr ptr) : base(ptr) {{{content}}}");
        file.WriteLine($"\tinternal static {className} Construct(IntPtr ptr) => new (ptr);");
        file.WriteLine();

        file.WriteLine($"\tpublic new static StringName __godot_name => *(StringName*)GDExtensionInterface.CreateStringName(\"{className}\");");
        for (var i = 0; i < methodRegistrations.Count; i++)
        {
            file.WriteLine($"\tstatic IntPtr __methodPointer{i} => {methodRegistrations[i]};");
        }
        file.WriteLine();
        file.WriteLine("\tpublic new static void Register() {");
        file.WriteLine($"\t\tif (!RegisterConstructor(\"{className}\", Construct)) return;");
        if (inherits is not null)
        {
            file.WriteLine($"\t\tGodotSharpGDExtension.{inherits}.Register();");
        }
        file.WriteLine("\t}");
        file.WriteLine("}");
        file.Close();
    }

    private void Variant()
    {
        Api.Enum type = default;
        Api.Enum operators = default;
        foreach (Api.Enum e in api.globalEnums)
        {
            switch (e.name)
            {
                case "Variant.Type":
                    type = e;
                    break;
                case "Variant.Operator":
                    operators = e;
                    break;
            }
        }
        type.name = "Type";
        operators.name = "Operator";
        StreamWriter file = File.CreateText(csDir + "/" + "Variant.cs");
        file.WriteLine("namespace GodotSharpGDExtension;");
        file.WriteLine();
        file.WriteLine("public sealed unsafe partial class Variant {");
        file.WriteLine();

        var types = new string[type.values.Length - 1];

        foreach (Api.Enum e in new[] { type, operators })
        {
            int prefixLength = Fixer.SharedPrefixLength(e.values.Select(x => x.name).ToArray());

            file.WriteLine($"\tpublic enum {Fixer.CSType(e.name, api)} {{");
            for (var i = 0; i < e.values.Length; i++)
            {
                Api.Value v = e.values[i];

                string name = Fixer.SnakeToPascal(v.name[prefixLength..]);
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
                file.WriteLine($"\t\t{name} = {v.value},");
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
