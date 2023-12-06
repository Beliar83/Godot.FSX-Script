using System.Diagnostics;
using System.Text.RegularExpressions;
using CaseExtensions;

namespace SharpGenerator;
public static class Fixer
{

    public static string GodotCppType(string name, Api api)
    {
        // TODO: Remove?
        // if (name.StartsWith("enum::"))
        // {
        //     name = name[6..];
        //     if (name.Contains('.'))
        //     {
        //         var className = name[..name.IndexOf('.')];
        //         var enumName = name[(name.IndexOf('.') + 1)..];
        //         bool isBuiletinClass = api.builtinClasses.Where(x => x.name == className).Cast<Api.BuiltinClass?>().FirstOrDefault() is not null;
        //         if (className != "Variant" && !isBuiletinClass)
        //         {
        //             var amount = api.classes.Where(x => x.name == className).First().enums.Where(x => x.name == enumName).Count();
        //             if (amount == 0)
        //             {
        //                 Program.Warn($"ENUM {name} not found");
        //                 return "long";
        //             }
        //         }
        //     }
        // }
        // if (name.Contains("typedarray::"))
        // {
        //     return $"Array<{CSType(name[12..], api)}>";
        // }
        // name = name.Replace("::", ".");
        // if (name.Contains("VariantType."))
        // {
        //     return "Variant";
        // }
        // if (name.StartsWith("bitfield::")) { name = name.Replace("bitfield.", ""); }
        // if (name.StartsWith("uint64_t")) { name = name.Replace("uint64_t", "UInt64"); }
        // if (name.StartsWith("uint16_t")) { name = name.Replace("uint16_t", "UInt16"); }
        // if (name.StartsWith("uint8_t")) { name = name.Replace("uint8_t", "byte"); }
        // if (name.StartsWith("int32_t")) { name = name.Replace("int32_t", "int"); }
        // if (name.StartsWith("real_t")) { name = name.Replace("real_t", "float"); }
        // if (name.StartsWith("int")) { name = name.Replace("int", "long"); }
        // if (name.StartsWith("VariantType")) { name = name.Replace("VariantType", "Variant.Type"); }

        if (name.Equals("float"))
        {
            name = "double";
            
        }

        if (name.Equals("int"))
        {
            name = "GDExtensionInt"; 
            
        }        
        
        if (name.Equals("int32_t"))
        {
            name = "int32_t"; 
            
        }

        if (name.Equals("bool"))
        {
            name = "GDExtensionBool";
        }

        return CanBePassedByValue(name) ? name : "GodotType";
    }

    public static (string csType, bool partial) CSType(string godotType, Api api)
    {
        if (godotType.StartsWith("enum::"))
        {
            godotType = godotType[6..];
            if (godotType.Contains('.'))
            {
                var className = godotType[..godotType.IndexOf('.')];
                var enumName = godotType[(godotType.IndexOf('.') + 1)..];
                bool isBuiltinClass = api.BuiltinClasses.Where(x => x.Name == className).Cast<Api.BuiltinClass?>().FirstOrDefault() is not null;
                if (className != "Variant" && !isBuiltinClass)
                {
                    var amount = api.Classes.First(x => x.Name == className).Enums.Count(x => x.Name == enumName);
                    if (amount == 0)
                    {
                        Program.Warn($"ENUM {godotType} not found");
                        return ("long", false);
                    }
                }
            }
        }
        godotType = godotType.Replace("const ", "");
        if (godotType.Contains("typedarray::"))
        {
            return ($"Array<{CSType(godotType[12..], api)}>", false);
        }
        godotType = godotType.Replace("::", ".");
        if (godotType.Contains("VariantType."))
        {
            return ("Variant", false);
        }

        if (godotType.StartsWith("bitfield.")) { godotType = godotType.Replace("bitfield.", ""); }
        if (godotType.StartsWith("uint64_t")) { godotType = godotType.Replace("uint64_t", "UInt64"); }
        if (godotType.StartsWith("uint16_t")) { godotType = godotType.Replace("uint16_t", "UInt16"); }
        if (godotType.StartsWith("uint8_t")) { godotType = godotType.Replace("uint8_t", "byte"); }
        if (godotType.StartsWith("int32_t")) { godotType = godotType.Replace("int32_t", "int"); }
        if (godotType.StartsWith("real_t")) { godotType = godotType.Replace("real_t", "float"); }
        if (godotType.StartsWith("float")) { godotType = godotType.Replace("float", "double"); }
        if (godotType.StartsWith("int")) { godotType = godotType.Replace("int", "long"); }
        if (godotType.Equals("String", StringComparison.InvariantCultureIgnoreCase)) { return ("String", true); }
        if (godotType.StartsWith("VariantType")) { godotType = godotType.Replace("VariantType", "Variant.Type"); }

        return (godotType == "Object" ? "GodotObject" : godotType, false);
    }

    private static readonly Dictionary<string, string> FunctionMapping = new()
    {
        { "nocasecmp", "NoCaseCompare" },
        { "casecmp", "CaseCompare" },
        { "printerr", "PrintErr" },
        { "printt", "PrintT" },
        { "prints", "PrintS" },
        { "printraw", "PrintRaw" },
        { "GetType", "GetGodotType" },
    };
    
    public static string MethodName(string name)
    {
        var res = "";
        string[] parts = name.Split(".");
        for (var i = 0; i < parts.Length - 1; i++)
        {
            res += parts[i] + ".";
        }
        string last = parts[^1];
        foreach ((string? godotName, string? csName) in FunctionMapping)
        {
            last = last.Replace(godotName, csName, StringComparison.InvariantCultureIgnoreCase);
        }

        return (res + last).ToSnakeCaseWithGodotAbbreviations();
    }

    public static string Name(string name)
    {
        return name switch
        {
            "object" => "@object",
            "base" => "@base",
            "interface" => "@interface",
            "class" => "@class",
            "default" => "@default",
            "char" => "@char",
            "string" => "@string",
            "event" => "@event",
            "lock" => "@lock",
            "operator" => "@operator",
            "enum" => "@enum",
            "in" => "@in",
            "out" => "@out",
            "checked" => "@checked",
            "override" => "@override",
            "new" => "@new",
            "params" => "@params",
            "internal" => "@internal",
            "bool" => "@bool",
            _ => name,
        };
    }

    public static string VariantOperatorEnum(this string operatorName)
    {
        return $"GDEXTENSION_VARIANT_OP_{operatorName.VariantOperatorCSharp().ToSnakeCase().ToUpper()}";
    }    
    
    public static string VariantOperatorCpp(this string operatorName)
    {
        return operatorName.VariantOperatorCSharp().ToSnakeCase();
    }
    
    public static string VariantOperatorCSharp(this string operatorName)
    {
        return operatorName switch
        {
            "==" => "Equal",
            "!=" => "NotEqual",
            "<" => "Less",
            "<=" => "LessEqual",
            ">" => "Greater",
            ">=" => "GreaterEqual",
            /* mathematic */
            "+" => "Add",
            "-" => "Subtract",
            "*" => "Multiply",
            "/" => "Divide",
            "unary-" => "Negate",
            "unary+" => "Positive",
            "%" => "Module",
            "**" => "Power",
            /* bitwise */
            "<<" => "ShiftLeft",
            ">>" => "ShiftRight",
            "&" => "BitAnd",
            "|" => "BitOr",
            "^" => "BitXor",
            "!" => "BitNegate",
            /* logic */
            "and" => "And",
            "or" => "Or",
            "xor" => "Xor",
            "not" => "Not",
            /* containment */
            "in" => "In",
            _ => operatorName,
        };
    }

    public static bool CanBePassedByValue(string type)
    {
        if (type.StartsWith("int")) return true;
        
        return type switch
        {
            "GDExtensionBool" => true,
            "GDExtensionInt" => true,
            "float" => true,
            "double" => true,
            "bool" => true,
            "GodotString" => true,
            _ => false,
        };
    }
    
    /// <summary>
    /// Get tuple with data for conversion FROM dotnet
    /// </summary>
    /// <param name="godotType">The godot cpp type to convert</param>
    /// <returns>ValueTuple with 3 members
    /// cppTypeForDotnetInterop: The cpp type that is used in the interface to dotnet
    /// conversion: The code to convert the type to cpp.
    /// </returns>
    public static (string cppTypeForDotnetInterop, string? destruction) GetDestructorDataForType(string godotType)
    {
        return godotType switch
        {
            _ => ("GodotType", null),
        };
    }        
    
    /// <summary>
    /// Get tuple with data for conversion FROM dotnet
    /// </summary>
    /// <param name="cppType">The godot cpp type to convert</param>
    /// <returns>ValueTuple with 3 members
    /// cppTypeForDotnetInterop: The cpp type that is used in the interface to dotnet
    /// conversion: The code to convert the type to cpp.
    /// canBePassedByValue: Whether the type can be passed by value.
    /// canConvertedBePassedByValue: Whether the converted type can be passed by value
    /// </returns>
    public static (string cppTypeForDotnetInterop, string conversion, bool canBePassedByValue, bool canConvertedBePassedByValue) GetConvertFromDotnetDataForType(string cppType)
    {
        return cppType switch
        {
            "GodotString" => (cppType, "convert_string_from_dotnet({0})", true, false),
            "GDExtensionBool" => ("bool", "convert_bool_from_dotnet({0})", true, true),
            _ => CanBePassedByValue(cppType) 
                ? (cppType, "{0}", true, true) 
                : ("GodotType", "{0}.pointer", false, false),
        };
    }    
    
    /// <summary>
    /// Get tuple with data for conversion TO dotnet
    /// </summary>
    /// <param name="cppType">The godot cpp type to convert</param>
    /// <returns>ValueTuple with 3 members
    /// cppTypeForDotnetInterop: The cpp type that is used in the interface to dotnet
    /// conversion: The code to convert the type to cpp.
    /// canDotnetBePassedByValue: Whether the dotnet type can be passed by value.
    /// canGodotBePassedByValue: Whether the godot type can be passed by value.
    /// </returns>
    public static (string cppTypeForDotnetInterop, string conversion, string construction, bool canDotnetBePassedByValue, bool canGodotBePassedByValue) GetConvertToDotnetDataForType(string cppType)
    {
        return cppType switch
        {
            "Variant" => (cppType, "{0}", $"auto {{1}} = new uint8_t[{Convert.BuiltinClassSizes["Variant"]}]" , true, false),
            "GodotString" => (cppType, "convert_string_to_dotnet({0})", "auto {1} = string_constructor()" , true, false),
            "GDExtensionBool" => ("bool", "convert_bool_to_dotnet({0})", "{0} {1} = {{ }};" , true, true),
            _ => CanBePassedByValue(cppType) 
                ? (cppType, "{0}", "{0} {1} = {{ }};",  true, true) : ("GodotType", "{0}", "auto {1} = {0}_constructor();", false, false),
        };
    }

    /// <summary>
    /// Returns a string to construct the godot type in c++
    /// </summary>
    /// <param name="type">The name of the type as used in godot.</param>
    /// <returns>A string with format placeholders. {0} = type, {1} = variable name </returns>
    public static string GetConstructionForGodotType(string type)
    {
        return type switch
        {
            "Variant" => $"auto {{1}} = GodotType {{{{ new uint8_t[{Convert.BuiltinClassSizes["Variant"]}] }}}};",
            "Object" => "auto {1} = GodotType {{ new godot::Object }}; // TODO: This needs testing",
            "bool" => "{0} {1} = {{ }};",
            "float" => "{0} {1} = {{ }};",
            "int" => "{0} {1} = {{ }};",
            _ =>  $"auto {{1}} = {type.ToSnakeCaseWithGodotAbbreviations()}_constructor();",
        };
    }
    
    public static string VariantName(string name)
    {
        return name switch
        {
            "int" => "Int",
            "long" => "Int",
            "float" => "Float",
            "double" => "Float",
            "bool" => "Bool",
            "Object" => "GodotObject",
            _ => name,
        };
    }

    public static string Value(string value)
    {
        if (value.Contains('('))
        {
            value = "new " + value;
        };
        value = value.Replace("inf", "double.PositiveInfinity");
        return value;
    }

    // public static readonly List<string> Words = new();
    // public static readonly Dictionary<string, string> SnakeCaseWords = new();
    private static int abbreviationCount = 0;
    public static readonly Dictionary<string, string> Abbreviations = new()
    {
        {"OpenXRIP", $"{{{abbreviationCount++}}}"},
        {"JSONRPC", $"{{{abbreviationCount++}}}"},
        {"FABRIK", $"{{{abbreviationCount++}}}"},
        {"SPIRV", $"{{{abbreviationCount++}}}"},
        {"HTTP", $"{{{abbreviationCount++}}}"},
        {"MIDI", $"{{{abbreviationCount++}}}"},
        {"JSON", $"{{{abbreviationCount++}}}"},
        {"IK3D", $"{{{abbreviationCount++}}}"},
        {"CDIK", $"{{{abbreviationCount++}}}"},
        {"AABB", $"{{{abbreviationCount++}}}"},
        {"UPNP", $"{{{abbreviationCount++}}}"},
        {"DTLS", $"{{{abbreviationCount++}}}"},
        {"GLTF", $"{{{abbreviationCount++}}}"},
        {"UInt", $"{{{abbreviationCount++}}}"},
        {"TLS", $"{{{abbreviationCount++}}}"},
        {"RID", $"{{{abbreviationCount++}}}"},
        {"AES", $"{{{abbreviationCount++}}}"},
        {"ZIP", $"{{{abbreviationCount++}}}"},
        {"RTC", $"{{{abbreviationCount++}}}"},
        {"MP3", $"{{{abbreviationCount++}}}"},
        {"WAV", $"{{{abbreviationCount++}}}"},
        {"CPU", $"{{{abbreviationCount++}}}"},
        {"GPU", $"{{{abbreviationCount++}}}"},
        {"CSG", $"{{{abbreviationCount++}}}"},
        {"VCS", $"{{{abbreviationCount++}}}"},
        {"DOF", $"{{{abbreviationCount++}}}"},
        {"SDF", $"{{{abbreviationCount++}}}"},
        {"MAC", $"{{{abbreviationCount++}}}"},
        {"UDP", $"{{{abbreviationCount++}}}"},
        {"TCP", $"{{{abbreviationCount++}}}"},
        {"API", $"{{{abbreviationCount++}}}"},
        {"PCK", $"{{{abbreviationCount++}}}"},
        {"UID", $"{{{abbreviationCount++}}}"},
        {"XML", $"{{{abbreviationCount++}}}"},
        {"IK", $"{{{abbreviationCount++}}}"},
        {"1D", $"{{{abbreviationCount++}}}"},
        {"2D", $"{{{abbreviationCount++}}}"},
        {"3D", $"{{{abbreviationCount++}}}"},
        {"XR", $"{{{abbreviationCount++}}}"},
        {"VR", $"{{{abbreviationCount++}}}"},
        {"GI", $"{{{abbreviationCount++}}}"},
        {"EQ", $"{{{abbreviationCount++}}}"},
        {"FX", $"{{{abbreviationCount++}}}"},
        {"DB", $"{{{abbreviationCount++}}}"},
        {"ID", $"{{{abbreviationCount++}}}"},
        {"GD", $"{{{abbreviationCount++}}}"},
        {"IP", $"{{{abbreviationCount++}}}"},
        {"OS", $"{{{abbreviationCount++}}}"},
        {"RD", $"{{{abbreviationCount++}}}"},
        {"UV", $"{{{abbreviationCount++}}}"},
    };

    public static string ToScreamingCaseSnakeWithGodotAbbreviations(this string name)
    {
        name = ApplySnakeCaseWithGodotAbbreviationsBase(name);

        return name.ToUpperInvariant();
    }    
    
    public static string ToSnakeCaseWithGodotAbbreviations(this string name)
    {
        name = ApplySnakeCaseWithGodotAbbreviationsBase(name);

        return name.ToLowerInvariant();
    }

    private static string ApplySnakeCaseWithGodotAbbreviationsBase(string name)
    {
        // if (name.Contains("2d", StringComparison.InvariantCultureIgnoreCase)) Debugger.Break();
        foreach ((string abbreviation, string formatSpecifier) in Abbreviations)
        {
            name = name.Replace(abbreviation, formatSpecifier);
        }

        name = name.ToSnakeCase();

        return string.Format(name, Abbreviations.Keys.ToArray());
    }

    public static string ToPascalCaseWithGodotAbbreviations(this string name)
    {
        // if (name.Contains("packedfloat", StringComparison.CurrentCultureIgnoreCase)) Debugger.Break();
        foreach ((string abbreviation, string formatSpecifier) in Abbreviations)
        {
            name = name.Replace(abbreviation, formatSpecifier, StringComparison.OrdinalIgnoreCase);
            // if (name.Equals(abbreviation.ToLower()))
            // {
            //     name = formatSpecifier;
            //     break;
            // }
            // name = name.Replace($"_{abbreviation.ToLower()}_", $"_{formatSpecifier}_");
            // if (name.StartsWith($"{abbreviation.ToLower()}_"))
            // {
            //     name = $"{formatSpecifier}{name[abbreviation.Length..]}";
            // }
            //
            // if (name.EndsWith($"_{abbreviation.ToLower()}"))
            // {
            //     name = $"{name[..^abbreviation.Length]}{formatSpecifier}";
            // }
        }

        name = name.ToPascalCase();

        return string.Format(name, Abbreviations.Keys.ToArray());
    }

    public static string CppArgumentName(string argumentName)
    {
        var cppKeywords = new Dictionary<string, string> 
        { 
            {"char", "character"}, 
            {"default", "default_value"}, 
        };

        return 
            cppKeywords.TryGetValue(argumentName, out string? fixedArgumentName) 
            ? fixedArgumentName 
            : argumentName;
    }
    
    public static string CSArgumentName(string argumentName)
    {
        var csKeywords = new List<string> 
        { 
            "base",
            "string",
        };

        return csKeywords.Contains(argumentName) 
            ? $"@{argumentName}" 
            : argumentName;
    }    
    
    public static int SharedPrefixLength(string[] names)
    {
        for (var l = 0; true; l++)
        {
            if (l >= names[0].Length) { return 0; }
            var c = names[0][l];
            foreach (var name in names)
            {
                if (l >= name.Length)
                {
                    return 0;
                }
                if (name[l] != c)
                {
                    return l;
                }
            }
        }
    }

    static string XMLConstant(string value)
    {
        value = value.ToPascalCase();
        return value switch
        {
            "@gdscript.nan" => "NaN",
            "@gdscript.tau" => "Tau",
            _ => value,
        };
    }

    static readonly (string, MatchEvaluator)[] xmlReplacements = new (string, MatchEvaluator)[] {
        (@"<", x => "&lt;"),
        (@">", x => "&gt;"),
        (@"&", x => "&amp;"),
        (@"\[b\](?<a>.+?)\[/b\]", x => $"<b>{x.Groups["a"].Captures[0].Value}</b>"),
        (@"\[i\](?<a>.+?)\[/i\]", x => $"<i>{x.Groups["a"].Captures[0].Value}</i>"),
        (@"\[constant (?<a>\S+?)\]", x => $"<see cref=\"{XMLConstant(x.Groups["a"].Captures[0].Value)}\"/>"),
        (@"\[code\](?<a>.+?)\[/code\]", x => $"<c>{x.Groups["a"].Captures[0].Value}</c>"),
        (@"\[param (?<a>\S+?)\]",x => $"<paramref name=\"{x.Groups["a"].Captures[0].Value}\"/>"),
        (@"\[method (?<a>\S+?)\]", x => $"<see cref=\"{MethodName(x.Groups["a"].Captures[0].Value)}\"/>"),
        (@"\[member (?<a>\S+?)\]", x => $"<see cref=\"{x.Groups["a"].Captures[0].Value}\"/>"),
        (@"\[enum (?<a>\S+?)\]",x => $"<see cref=\"{x.Groups["a"].Captures[0].Value}\"/>"),
        (@"\[signal (?<a>\S+?)\]", x => $"<see cref=\"EmitSignal{x.Groups["a"].Captures[0].Value.ToPascalCase()}\"/>"), //currently just two functions
		(@"\[theme_item (?<a>\S+?)\]", x => $"<see cref=\"{x.Groups["a"].Captures[0].Value}\"/>"), //no clue
		//(@"cref=""Url=\$docsUrl/(?<a>.+?)/>", x => $"href=\"https://docs.godotengine.org/en/stable/{x.Groups["a"].Captures[0].Value}\"/>"),
		(@"\[url=(?<a>.+?)\](?<b>.+?)\[/url]", x => $"<see href=\"{x.Groups["a"].Captures[0].Value}\">{x.Groups["b"].Captures[0].Value}</see>"),
        (@"\[(?<a>\S+?)\]", x => $"<see cref=\"{x.Groups["a"].Captures[0].Value}\"/>"), //can be multiple things
	};

    public static string XMLComment(string comment, int indent = 1)
    {

        var tabs = new string('\t', count: indent);
        var result = tabs + "/// <summary>\n";
        var lines = comment.Trim().Split("\n");

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.Contains("[codeblock]"))
            {
                var offset = lines[i].Count(x => x == '\t');
                result += tabs + "/// <code>\n";
                i += 1;
                line = lines[i][offset..];
                while (line.Contains("[/codeblock]") == false)
                {
                    i += 1;
                    result += tabs + "/// " + line + "\n";
                    while (lines[i].Length <= offset) { i += 1; }
                    line = lines[i][offset..];
                }
                result += tabs + "/// </code>\n";
            }
            else if (line.Contains("[codeblocks]"))
            {
                while (line.Contains("[/codeblocks]") == false)
                {
                    i += 1;
                    line = lines[i].Trim();
                    if (line.Contains("[csharp]"))
                    {
                        var offset = lines[i].Count(x => x == '\t');
                        result += tabs + "/// <code>\n";
                        i += 1;
                        line = lines[i][offset..];
                        while (line.Contains("[/csharp]") == false)
                        {
                            i += 1;
                            result += tabs + "/// " + line + "\n";
                            while (lines[i].Length <= offset) { i += 1; }
                            line = lines[i][offset..];
                        }
                        result += tabs + "/// </code>\n";
                    }
                }
            }
            else
            {
                foreach (var (pattern, replacement) in xmlReplacements)
                {
                    line = Regex.Replace(line, pattern, replacement);
                }
                result += tabs + "/// " + line + "<br/>" + "\n";
            }
        }
        result += tabs + "/// </summary>";
        return result.ReplaceLineEndings();
    }

    public static string VariantEnumType(this string type)
    {
        // if (type.Contains("packed", StringComparison.InvariantCultureIgnoreCase)) Debugger.Break();
        var variantEnumType = $"GDEXTENSION_VARIANT_TYPE_{type.ToScreamingCaseSnakeWithGodotAbbreviations()}";
        return variantEnumType;
    }
}
