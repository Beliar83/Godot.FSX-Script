using System.Xml.Serialization;
using SharpGenerator.Documentation;

namespace SharpGenerator;

public class Convert
{
    readonly HashSet<string> objectTypes = new() { "Variant" };
    readonly HashSet<string> builtinObjectTypes = new() {
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
    readonly Dictionary<string, HashSet<string>> registrations = new()
    {
        ["builtin"] = new() { "StringName", "Variant" },
        ["utility"] = new(),
        ["core"] = new(),
        ["servers"] = new(),
        ["scene"] = new(),
        ["editor"] = new(),
    };
    readonly Api api;
    readonly XmlSerializer classXml;
    readonly XmlSerializer builtinXml;
    readonly string dir;
    private readonly string docDir;
    readonly string configName;

    public Convert(Api api, string dir, string docDir, string configName)
    {
        this.api = api;
        this.classXml = new XmlSerializer(typeof(Documentation.Class));
        this.builtinXml = new XmlSerializer(typeof(Documentation.BuiltinClass));
        this.dir = dir;
        this.docDir = docDir;
        this.configName = configName;
    }

    public void Emit()
    {

        foreach (var o in builtinObjectTypes)
        {
            objectTypes.Add(o);
        }

        foreach (var c in api.classes)
        {
            objectTypes.Add(c.name);
        }

        BuiltinClasses();
        Classes();

        Directory.CreateDirectory(dir + "/Enums");
        foreach (var e in api.globalEnums)
        {
            GlobalEnum(e, dir + "/Enums");
        }

        Directory.CreateDirectory(dir + "/NativeStructures");
        foreach (var native in api.nativeStructures)
        {
            var file = File.CreateText(dir + "/NativeStructures/" + Fixer.Type(native.name, api) + ".cs");
            file.WriteLine("namespace GodotSharpGDExtension;");
            file.WriteLine(value: "[StructLayout(LayoutKind.Sequential)]");
            file.WriteLine($"public unsafe struct {native.name} {{");
            foreach (var member in native.format.Split(";"))
            {
                var pair = member.Split(" ");
                var name = Fixer.Name(pair[1]);
                var type = Fixer.Type(pair[0], api);
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
                    var size = int.Parse(name.Split("[")[1].Split("]")[0]);
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
        Directory.CreateDirectory(dir + "/UtilityFunctions");
        var docGlobalScope = GetDocs("@GlobalScope");
        var files = new Dictionary<string, (StreamWriter, List<string>)>();
        foreach (var f in api.untilityFunction)
        {
            var cat = string.Concat(f.category![0].ToString().ToUpper(), f.category.AsSpan(1));
            if (files.TryGetValue(cat, out var file) == false)
            {
                file = (File.CreateText(dir + "/UtilityFunctions/" + cat + ".cs"), new List<string>());
                files.Add(cat, file);
                file.Item1.WriteLine("namespace GodotSharpGDExtension;");
                file.Item1.WriteLine($"public static unsafe partial class {cat} {{");
                registrations["utility"].Add(cat);
            }
            Documentation.Method d = null;
            if (docGlobalScope != null && docGlobalScope.methods != null)
            {
                d = docGlobalScope.methods.FirstOrDefault(x => x.name == f.name);
            }
            Method(f, "", file.Item1, MethodType.Utility, file.Item2, d);
        }
        foreach (var (_, (file, list)) in files)
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

        var register = File.CreateText(dir + "/Register.cs");
        register.WriteLine("namespace GodotSharpGDExtension;");
        register.WriteLine("public static class Register {");
        foreach (var (key, list) in registrations)
        {
            register.WriteLine($"\tpublic static void Register{Fixer.SnakeToPascal(key)}() {{");
            foreach (var r in list)
            {
                register.WriteLine($"\t\t{r}.Register();");
            }
            register.WriteLine("\t}");
        }
        register.WriteLine("}");
        register.Close();

        Variant();
    }

    Documentation.Class GetDocs(string name)
    {
        if (docDir == null) { return null; }
        var path = docDir + name + ".xml";
        if (File.Exists(path))
        {
            var file = File.OpenRead(path);
            var d = (Documentation.Class)classXml.Deserialize(file)!;
            file.Close();
            return d;
        }
        else
        {
            return null;
        }
    }

    Documentation.BuiltinClass GetBuiltinDocs(string name)
    {
        if (docDir == null) { return null; }
        var path = docDir + name + ".xml";
        if (File.Exists(path))
        {
            var file = File.OpenRead(path);
            var d = (Documentation.BuiltinClass)builtinXml.Deserialize(file)!;
            file.Close();
            return d;
        }
        else
        {
            return null;
        }
    }

    void GlobalEnum(Api.Enum e, string dir)
    {
        if (e.name.Contains('.')) { return; }
        var name = Fixer.Type(e.name, api).Replace(".", "");
        var file = File.CreateText(dir + "/" + Fixer.Type(name, api) + ".cs");
        file.WriteLine("namespace GodotSharpGDExtension {");
        Enum(e, file);
        file.WriteLine("}");
        file.Close();
    }

    void BuiltinClasses()
    {
        var dir = this.dir + "/BuiltinClasses";
        Directory.CreateDirectory(dir);
        foreach (var c in api.builtinClasses)
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
                    BuiltinClass(c, dir, builtinObjectTypes.Contains(c.name));
                    break;
            }
        }
    }

    void BuiltinClass(Api.BuiltinClass c, string dir, bool hasPointer)
    {
        string className = c.name;
        if (className == "Object") className = "GodotObject";
        var file = File.CreateText(dir + "/" + Fixer.Type(className, api) + ".cs");
        registrations["builtin"].Add(Fixer.Type(className, api));

        var doc = GetBuiltinDocs(className);

        var constructorRegistrations = new List<string>();
        var operatorRegistrations = new List<string>();
        var methodRegistrations = new List<string>();

        int size = -1;
        foreach (var config in api.builtinClassSizes)
        {
            if (config.buildConfiguration == configName)
            {
                foreach (var sizePair in config.sizes)
                {
                    if (sizePair.name == className)
                    {
                        size = sizePair.size;
                        break;
                    }
                }
                break;
            }
        }

        file.WriteLine("namespace GodotSharpGDExtension;");
        file.WriteLine();
        if (hasPointer == false)
        {
            file.WriteLine($"[StructLayout(LayoutKind.Explicit, Size = {size})]");
        }
        file.WriteLine($"public unsafe partial {(hasPointer ? "class" : "struct")} {Fixer.Type(className, api)} {{");
        file.WriteLine();
        file.WriteLine("\tstatic private bool _registered = false;");
        file.WriteLine();

        if (hasPointer)
        {
            file.WriteLine($"\tpublic const int StructSize = {size};");
            file.WriteLine("\tpublic IntPtr internalPointer;");
            file.WriteLine($"\tpublic {className}(IntPtr ptr) => internalPointer = ptr;");
            file.WriteLine();
        }

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
            foreach (var member in c.members)
            {
                Documentation.Member d = null;
                if (doc != null && doc.members != null)
                {
                    d = doc.members.FirstOrDefault(x => x.name == member.name);
                }
                Member(api, c, member, configName, file, membersWithFunctions, d);
            }
        }

        if (c.constants != null)
        {
            foreach (var con in c.constants)
            {
                if (doc != null && doc.constants != null)
                {
                    var d = doc.constants.FirstOrDefault(x => x.name == con.name);
                    if (d != null && d.comment != null)
                    {
                        var com = Fixer.XMLComment(d.comment);
                        file.WriteLine(com);
                    }
                }
                file.WriteLine($"\tpublic static {Fixer.Type(con.type, api)} {Fixer.SnakeToPascal(con.name)} {{ get; private set; }}");
            }
            file.WriteLine();
        }

        if (c.constructors != null)
        {
            for (var i = 0; i < c.constructors.Length; i++)
            {
                var constructor = c.constructors[i];
                Documentation.Constructor d = null;
                if (doc != null && doc.constructors != null)
                {
                    d = doc.constructors[i];
                }
                Constructor(c, constructor, file, constructorRegistrations, d, hasPointer);
            }
        }
        else
        {
            var emptyApiConstructor = new Api.Constructor { arguments = Array.Empty<Api.Argument>(), index = 0};
            var emptyDocConstructor = new Constructor();
            Constructor(c, emptyApiConstructor, file, constructorRegistrations, emptyDocConstructor, hasPointer);
        }

        if (c.operators != null)
        {
            foreach (var op in c.operators)
            {
                Documentation.Operator d = null;
                if (doc != null && doc.operators != null)
                {
                    d = doc.operators.FirstOrDefault(x => x.name == $"operator {op.name}");
                }
                Operator(op, className, file, operatorRegistrations, d);
            }
        }

        if (c.enums != null)
        {
            foreach (var e in c.enums)
            {
                Enum(e, file, doc?.constants);
            }
        }

        if (c.methods != null)
        {
            foreach (var meth in c.methods)
            {
                Documentation.Method d = null;
                if (doc != null && doc.methods != null)
                {
                    d = doc.methods.FirstOrDefault(x => x.name == meth.name);
                }
                Method(meth, className, file, MethodType.Native, methodRegistrations, d, isBuiltinPointer: hasPointer);
            }
        }

        EqualAndHash(className, file);

        if (hasPointer)
        {
            file.WriteLine($"\t~{Fixer.Type(className, api)}() {{");
            //file.WriteLine($"\t\tif(internalPointer == null) {{ return; }}");
            file.WriteLine($"\t\tGDExtensionInterface.CallGDExtensionPtrDestructor(__destructor, internalPointer);");
            //file.WriteLine($"\t\tGDExtensionInterface.MemFree(internalPointer);");
            //file.WriteLine($"\t\tinternalPointer = null;");
            file.WriteLine($"\t}}");
            file.WriteLine();
            file.WriteLine("\t[StructLayout(LayoutKind.Explicit, Size = StructSize)]");
            file.WriteLine("\tpublic struct InternalStruct { }");
            file.WriteLine();
        }


        foreach (var member in membersWithFunctions)
        {
            file.WriteLine($"\tprivate static readonly StringName _stringName{member} = new (\"{member}\");");
            file.WriteLine($"\tstatic IntPtr {member}Getter = GDExtensionInterface.VariantGetPtrGetter((GDExtensionVariantType)Variant.Type.{Fixer.Type(className, api)}, _stringName{member}.internalPointer);");
            file.WriteLine($"\tstatic IntPtr {member}Setter = GDExtensionInterface.VariantGetPtrSetter((GDExtensionVariantType)Variant.Type.{Fixer.Type(className, api)}, _stringName{member}.internalPointer);");
        }
        for (var i = 0; i < constructorRegistrations.Count; i++)
        {
            file.WriteLine($"\tstatic IntPtr __constructorPointer{i} = {constructorRegistrations[i]};");
        }
        for (var i = 0; i < operatorRegistrations.Count; i++)
        {
            file.WriteLine($"\tstatic IntPtr __operatorPointer{i} = {operatorRegistrations[i]};");
        }
        for (var i = 0; i < methodRegistrations.Count; i++)
        {
            file.WriteLine($"\tstatic IntPtr __methodPointer{i} = {methodRegistrations[i]};");
        }

        if (hasPointer)
        {
            file.WriteLine($"\tstatic IntPtr __destructor = GDExtensionInterface.VariantGetPtrDestructor((GDExtensionVariantType)Variant.Type.{Fixer.Type(className, api)});");
        }

        file.WriteLine();

        file.WriteLine("\tpublic static void Register() {");
        file.WriteLine("\t\tif (_registered) return;");
        file.WriteLine("\t\t_registered = true;");
        if (hasPointer)
        {
            // file.WriteLine($"\t\t__destructor = GDExtensionInterface.VariantGetPtrDestructor((GDExtensionVariantType)Variant.Type.{Fixer.Type(className, api)});");
        }
        foreach (var member in membersWithFunctions)
        {
            // file.WriteLine($"\t\tvar _stringName{member} = new StringName(\"{member}\");");
            // file.WriteLine($"\t\t{member}Getter = GDExtensionInterface.VariantGetPtrGetter((GDExtensionVariantType)Variant.Type.{Fixer.Type(className, api)}, _stringName{member}.internalPointer);");
            // file.WriteLine($"\t\t{member}Setter = GDExtensionInterface.VariantGetPtrSetter((GDExtensionVariantType)Variant.Type.{Fixer.Type(className, api)}, _stringName{member}.internalPointer);");
        }
        for (var i = 0; i < operatorRegistrations.Count; i++)
        {
            // file.WriteLine(operatorRegistrations[i]);
        }
        if (c.constants != null)
        {
            foreach (var con in c.constants)
            {
                file.WriteLine($"\t\t{Fixer.SnakeToPascal(con.name)} = {Fixer.Value(con.value)};");
            }
        }
        file.WriteLine("\t}");
        file.WriteLine("}");
        file.Close();
    }

    static void Member(Api api, Api.BuiltinClass c, Api.Member member, string configName, StreamWriter file, List<string> withFunctions, Documentation.Member doc)
    {
        var offset = -1;
        foreach (var config in api.builtinClassMemberOffsets)
        {
            if (config.buildConfiguration == configName)
            {
                foreach (var cl in config.classes)
                {
                    if (cl.name == c.name)
                    {
                        foreach (var memberOffset in cl.members)
                        {
                            if (memberOffset.member == member.name)
                            {
                                offset = memberOffset.offset;
                            }
                        }
                    }
                }
            }
        }
        if (doc != null)
        {
            var com = Fixer.XMLComment(doc.comment);
            file.WriteLine(com);
        }
        if (offset >= 0)
        {
            file.WriteLine($"\t[FieldOffset({offset})]");
            file.WriteLine($"\tpublic {member.type} {member.name};");
        }
        else
        {
            // builtinObjectTypes.Contains()
            string ret;
            if (api.builtinClasses.Any(c => c.name == member.type))
            {
                ret = $"Marshal.PtrToStructure<{member.type}>(res)";
            }
            else
            {
                ret = $"new {member.type}(res)";
            }
            file.WriteLine($$"""
				public {{member.type}} {{member.name}} {
					get {
						IntPtr res;
						fixed ({{Fixer.Type(c.name, api)}}* ptr = &this) {
							res = GDExtensionInterface.CallGDExtensionPtrGetter({{member.name}}Getter, (IntPtr)ptr);
						}
						return {{ret}};
					}
					set {
						fixed ({{Fixer.Type(c.name, api)}}* ptr = &this) {
							GDExtensionInterface.CallGDExtensionPtrSetter({{member.name}}Setter, (IntPtr)ptr, (IntPtr)(&value));
						}
					}
				}
			""");
            withFunctions.Add(member.name);
        }
        file.WriteLine();
    }

    void Constructor(Api.BuiltinClass c, Api.Constructor constructor, StreamWriter file, List<string> constructorRegistrations, Documentation.Constructor doc, bool hasPointer)
    {
        if (doc != null)
        {
            var com = Fixer.XMLComment(doc.description);
            file.WriteLine(com);
        }
        file.Write($"\tpublic {Fixer.Type(c.name, api)}(");
        if (constructor.arguments != null)
        {
            for (var i = 0; i < constructor.arguments.Length - 1; i++)
            {
                var arg = constructor.arguments[i];
                file.Write(value: $"{Fixer.Type(arg.type, api)} {Fixer.Name(arg.name)}, ");
            }
            var a = constructor.arguments.Last();
            file.Write(value: $"{Fixer.Type(a.type, api)} {Fixer.Name(a.name)}");
        }
        file.WriteLine(") {");
        file.WriteLine("\t\tRegister();");
        if (hasPointer)
        {
            file.WriteLine("\t\tinternalPointer = GDExtensionInterface.MemAlloc(StructSize);");
            file.WriteLine("\t\t\t\tvar bytePointer = (byte*)internalPointer;\n\t\tfor (int i = 0; i < StructSize; i++)\n\t\t{\n\t\t\tbytePointer[i] = 0;\n\t\t}");

        }
        var m = $"__constructorPointer{constructorRegistrations.Count}";
        file.WriteLine($"\t\tvar constructor = {m};");
        constructorRegistrations.Add($"GDExtensionInterface.VariantGetPtrConstructor((GDExtensionVariantType)Variant.Type.{Fixer.Type(c.name, api)}, {constructor.index})");

        if (constructor.arguments != null)
        {
            file.WriteLine($"\t\tvar args = stackalloc IntPtr[{constructor.arguments.Length}];");
            for (var i = 0; i < constructor.arguments.Length; i++)
            {
                var arg = constructor.arguments[i];
                file.WriteLine($"\t\targs[{i}] = {ValueToPointer(Fixer.Name(arg.name), arg.type)};");
            }
        }
        if (hasPointer == false)
        {
            file.WriteLine($"\t\tfixed ({Fixer.Type(c.name, api)}* ptr = &this) {{");
            file.Write("\t\t\tGDExtensionInterface.CallGDExtensionPtrConstructor(constructor, (IntPtr)ptr, ");
        }
        else
        {
            file.Write("\t\tGDExtensionInterface.CallGDExtensionPtrConstructor(constructor, internalPointer, ");
        }
        if (constructor.arguments != null)
        {
            file.WriteLine("*args);");
        }
        else
        {
            file.WriteLine("IntPtr.Zero);");
        }
        if (hasPointer == false)
        {
            file.WriteLine("\t\t}");
        }
        file.WriteLine("\t}");
        file.WriteLine();
    }

    void Operator(Api.Operator op, string className, StreamWriter file, List<string> operatorRegistrations, Documentation.Operator doc)
    {
        if (op.rightType != null)
        {
            if (op.rightType == "Variant") { return; }
            var name = op.name switch
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
            file.WriteLine($"\tpublic static {Fixer.Type(op.returnType, api)} {name}({Fixer.Type(className, api)} left, {Fixer.Type(op.rightType, api)} right) {{");
            var m = $"__operatorPointer{operatorRegistrations.Count}";
            file.WriteLine($"\t\tvar __op = {m};");
            operatorRegistrations.Add($"GDExtensionInterface.VariantGetPtrOperatorEvaluator((GDExtensionVariantOperator)Variant.Operator.{Fixer.VariantOperator(op.name)}, (GDExtensionVariantType)Variant.Type.{className}, (GDExtensionVariantType)Variant.Type.{Fixer.VariantName(op.rightType)})");
            file.WriteLine($"\t\tIntPtr __res = GDExtensionInterface.CallGDExtensionPtrOperatorEvaluator(__op, {ValueToPointer("left", className)}, {ValueToPointer("right", op.rightType)});");
            file.WriteLine($"\t\treturn {ReturnStatementValue(op.returnType)};");
        }
        else
        {
            var name = op.name switch
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
            file.WriteLine($"\tpublic static {Fixer.Type(op.returnType, api)} {name}({Fixer.Type(className, api)} value) {{");
            var m = $"__operatorPointer{operatorRegistrations.Count}";
            file.WriteLine($"\t\tvar __op = {m};");
            operatorRegistrations.Add($"GDExtensionInterface.VariantGetPtrOperatorEvaluator((GDExtensionVariantOperator)Variant.Operator.{Fixer.VariantOperator(op.name)}, (GDExtensionVariantType)Variant.Type.{className}, (GDExtensionVariantType)Variant.Type.Nil)");
            file.WriteLine($"\t\tIntPtr __res = GDExtensionInterface.CallGDExtensionPtrOperatorEvaluator(__op, {ValueToPointer("value", className)}, IntPtr.Zero);");
            file.WriteLine($"\t\treturn {ReturnStatementValue(op.returnType)};");
        }
        file.WriteLine("\t}");
        file.WriteLine();
    }

    void Enum(Api.Enum e, StreamWriter file, Documentation.Constant[] constants = null)
    {
        var prefixLength = Fixer.SharedPrefixLength(e.values.Select(x => x.name).ToArray());
        if (e.isBitfield ?? false)
        {
            file.WriteLine($"\t[Flags]");
        }

        file.WriteLine($"\tpublic enum {Fixer.Type(e.name, api)} {{");
        foreach (var v in e.values)
        {
            if (constants != null)
            {
                var d = constants.FirstOrDefault(x => x.@enum != null && x.@enum == e.name && x.name == v.name);
                if (d != null && d.comment != null)
                {
                    file.WriteLine(Fixer.XMLComment(d.comment, 2));
                }
            }
            var name = Fixer.SnakeToPascal(v.name[prefixLength..]);
            if (char.IsDigit(name[0])) { name = "_" + name; }
            file.WriteLine($"\t\t{name} = {v.value},");
        }
        file.WriteLine("\t}");
        file.WriteLine();
    }

    string ValueToPointer(string name, string type)
    {
        var f = Fixer.Type(type, api);
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

    bool IsClassType(string type)
    {
        var f = Fixer.Type(type, api);
        return f == "Array" || f.StartsWith("Array<") || f == "Variant";
    }

    string ReturnLocationType(string type, string name)
    {
        var f = Fixer.Type(type, api);
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

    string ReturnStatementValue(string type)
    {
        var f = Fixer.Type(type, api);
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

    enum MethodType
    {
        Class,
        Native,
        Utility,
    }

    static bool IsValidDefaultValue(string value, string type)
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

    string FixDefaultValue(string value, string type)
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
        return $"({Fixer.Type(type, api)}){value}";
    }

    void Method(Api.Method meth, string className, StreamWriter file, MethodType type, List<string> methodRegistrations, Documentation.Method doc, bool isSingleton = false, bool isBuiltinPointer = false)
    {
        var header = "";
        if (doc != null)
        {
            if (doc.description != null)
            {
                header += Fixer.XMLComment(doc.description) + Environment.NewLine;
            }
        }
        header += "\tpublic ";
        var ret = meth.returnType ?? meth.returnValue?.type ?? "";
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
            header += Fixer.Type(ret, api);
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
                var arg = meth.arguments[i];
                var suffix = "";
                if (arg.defaultValue != null)
                {
                    var validDefault = true;
                    for (var j = i; j < meth.arguments.Length; j++)
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
                        for (var j = i; j < meth.arguments.Length; j++)
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
                header += $"{Fixer.Type(arg.type, api)} {Fixer.Name(arg.name)}{suffix}";
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
                var arg = meth.arguments[i];
                file.Write($"\t\t__args[{i}] = ");
                if (meth.isVararg)
                {
                    var val = arg.type != "Variant" ? $"new Variant({Fixer.Name(arg.name)})" : Fixer.Name(arg.name);
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
            var offset = meth.arguments != null ? $"{meth.arguments.Length} + " : "";
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

    void EqualAndHash(string className, StreamWriter file)
    {
        file.WriteLine("\tpublic override bool Equals(object obj) {");
        file.WriteLine("\t\tif (obj == null) { return false; }");
        file.WriteLine($"\t\tif (obj is {Fixer.Type(className, api)} other == false) {{ return false; }}");
        file.WriteLine("\t\treturn this == other;");
        file.WriteLine("\t}");
        file.WriteLine();

        //todo: based on members
        file.WriteLine("\tpublic override int GetHashCode() {");
        file.WriteLine("\t\treturn base.GetHashCode();");
        file.WriteLine("\t}");
        file.WriteLine();
    }

    Api.Method? GetMethod(string cName, string name)
    {
        foreach (var c in api.classes)
            if (cName == c.name)
            {
                if (c.methods != null)
                {
                    foreach (var m in c.methods!)
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

    void Classes()
    {
        var dir = this.dir + "/Classes";
        Directory.CreateDirectory(dir);
        foreach (var c in api.classes)
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

    void Class(Api.Class c, string dir)
    {
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

        var file = File.CreateText(dir + "/" + className + ".cs");
        registrations[c.apiType].Add(className);

        var doc = GetDocs(className);

        var methodRegistrations = new List<string>();

        file.WriteLine("namespace GodotSharpGDExtension;");
        file.WriteLine();
        file.Write("public unsafe ");
        var isSingleton = api.singletons.Any(x => x.type == className);
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
            foreach (var con in c.constants)
            {
                if (doc != null)
                {
                    var d = doc.constants.FirstOrDefault(x => x.name == con.name);
                    if (d != null && d.comment != null)
                    {
                        var com = Fixer.XMLComment(d.comment);
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
            foreach (var e in c.enums)
            {
                Enum(e, file, doc?.constants);
            }
        }

        var addedMethods = new List<Api.Method>();

        if (c.properties != null)
        {
            foreach (var prop in c.properties)
            {
                var type = prop.type;
                var cast = "";

                var getter = GetMethod(className, prop.getter);
                var Setter = GetMethod(className, prop.setter);

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
                    var d = doc.members.FirstOrDefault(x => x.name == prop.name);
                    if (d != null && d.comment != null)
                    {
                        var com = Fixer.XMLComment(d.comment);
                        file.WriteLine(com);
                    }
                }
                if (getter != null) { type = getter.Value.returnValue!.Value.type; } else { type = Setter!.Value.arguments![0].type; }

                var hasEnumOfSameName = (c.enums?.Where(x => x.name == Fixer.Name(Fixer.SnakeToPascal(prop.name))).FirstOrDefault())?.name != null;

                file.Write($"\tpublic {Fixer.Type(type, api)} {Fixer.Name(Fixer.SnakeToPascal(prop.name)) + (hasEnumOfSameName ? "Value" : "")} {{ ");

                if (prop.index.HasValue)
                {
                    if (getter == null)
                    {
                        throw new NotImplementedException("get cast from Setter");
                    }
                    cast = $"({Fixer.Type(getter.Value.arguments![0].type, api)})";
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

        foreach (var meth in addedMethods)
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
            foreach (var sig in c.signals)
            {
                file.Write($"\tpublic void EmitSignal{Fixer.SnakeToPascal(sig.name)}(");
                if (sig.arguments != null)
                {
                    for (var j = 0; j < sig.arguments.Length; j++)
                    {
                        var p = sig.arguments[j];
                        file.Write($"{Fixer.Type(p.type, api)} {Fixer.Name(p.name)}{(j < sig.arguments.Length - 1 ? ", " : "")}");
                    }
                }

                file.Write($") => EmitSignal(\"{sig.name}\"{(sig.arguments != null ? ", " : "")}");
                if (sig.arguments != null)
                {
                    for (var j = 0; j < sig.arguments.Length; j++)
                    {
                        var p = sig.arguments[j];
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
                        var p = sig.arguments[j];
                        file.Write($"{Fixer.Type(p.type, api)} {Fixer.Name(p.name)}{(j < sig.arguments.Length - 1 ? ", " : "")}");
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

        var content = className == "RefCounted" ? " Reference();\n" : "";

        content += "\tRegister();";

        file.WriteLine($"\tpublic {className}() : base(__godot_name) {{");
        file.WriteLine(content);
        file.WriteLine("}");
        file.WriteLine($"\tprotected {className}(StringName type) : base(type) {{{content}}}");
        file.WriteLine($"\tprotected {className}(IntPtr ptr) : base(ptr) {{{content}}}");
        file.WriteLine($"\tinternal static {className} Construct(IntPtr ptr) => new (ptr);");
        file.WriteLine();

        file.WriteLine($"\tpublic new static StringName __godot_name = new StringName(\"{className}\");");
        for (var i = 0; i < methodRegistrations.Count; i++)
        {
            file.WriteLine($"\tstatic IntPtr __methodPointer{i} = {methodRegistrations[i]};");
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

    void Variant()
    {
        Api.Enum type = default;
        Api.Enum operators = default;
        foreach (var e in api.globalEnums)
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
        var file = File.CreateText(dir + "/" + "Variant.cs");
        file.WriteLine("namespace GodotSharpGDExtension;");
        file.WriteLine();
        file.WriteLine("public sealed unsafe partial class Variant {");
        file.WriteLine();

        var types = new string[type.values.Length - 1];

        foreach (var e in new[] { type, operators })
        {
            var prefixLength = Fixer.SharedPrefixLength(e.values.Select(x => x.name).ToArray());

            file.WriteLine($"\tpublic enum {Fixer.Type(e.name, api)} {{");
            for (var i = 0; i < e.values.Length; i++)
            {
                var v = e.values[i];

                var name = Fixer.SnakeToPascal(v.name[prefixLength..]);
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
            var t = types[i];
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
            var t = types[i];
            if (t == "Object") t = "GodotObject";
            file.WriteLine($"\t\telse if (Variant.Type.{t} == valuetype) \n\t\t{{");
            file.WriteLine($"\t\t\treturn (object)({VariantTypeToCSharpType(t)})value;");
            file.WriteLine("\t\t}");
        }
        file.WriteLine("\t\telse \n\t\t{ \n\t\t\treturn null; \n\t\t} \n\t}");


        for (var i = 1; i < types.Length; i++)
        {
            var t = types[i];
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
            var t = types[i];
            file.Write("\tpublic Variant(");
            file.Write(VariantTypeToCSharpType(t));
            file.WriteLine(" value) : this() => SaveIntoPointer(value, internalPointer);");
        }
        file.WriteLine();

        for (var i = 1; i < types.Length; i++)
        {
            var t = types[i];
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
            var t = types[i];
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
            var t = types[i];
            file.Write("\tpublic static implicit operator Variant(");
            file.Write(VariantTypeToCSharpType(t));
            file.WriteLine(" value) => new Variant(value);");
        }
        file.WriteLine();

        for (var i = 1; i < types.Length; i++)
        {
            var t = types[i];
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
