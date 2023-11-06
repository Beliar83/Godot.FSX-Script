using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CppAst;

namespace SharpGenerator;

record FunctionParameter(int Index, string Name);

internal class Program
{
    public static string GodotRootDir;

    public static void Warn(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Warn: " + message);
        Console.ResetColor();
    }

    public static async Task Main(string[] args)
    {
        Console.WriteLine($"Working Dir {Directory.GetCurrentDirectory()}");
        string rootFolder = "../../../../../";
        
        string godotCppFolder = Path.GetFullPath($"{rootFolder}godot-cpp");
        Console.WriteLine($"GodotCpp Folder {godotCppFolder}");
        string cppFilename = Path.Join(godotCppFolder, "include/godot_cpp/godot.hpp");
        var cppParserOptions = new CppParserOptions();

        cppParserOptions.IncludeFolders.Add(Path.Join(godotCppFolder, "gdextension"));
        cppParserOptions.IncludeFolders.Add(Path.Join(godotCppFolder, "gen/include"));
        cppParserOptions.IncludeFolders.Add(Path.Join(godotCppFolder, "include"));
        CppCompilation compilation = CppParser.ParseFile(cppFilename, cppParserOptions);

        const string gdextensionInterfacePrefix = "gdextension_interface_";
        var interfaceFunctions = compilation
            .Fields
            .Where(f => f.Name.StartsWith(gdextensionInterfacePrefix)).ToList();

        var dotnetApiHeaderBuilder = new StringBuilder();
        var dotnetApiCodeBuilder = new StringBuilder();

        dotnetApiHeaderBuilder.AppendLine("#pragma once");
        dotnetApiHeaderBuilder.AppendLine();
        dotnetApiHeaderBuilder.AppendLine("#include \"gdextension_interface.h\"");
        dotnetApiHeaderBuilder.AppendLine();
        dotnetApiHeaderBuilder.AppendLine("GDExtensionClassLibraryPtr get_library();");
        // dotnetApiHeaderBuilder.AppendLine("struct interface_functions {");

        dotnetApiCodeBuilder.AppendLine("#include \"dotnet_api.h\"");
        dotnetApiCodeBuilder.AppendLine("#include \"godot_cpp/godot.hpp\"");
        dotnetApiCodeBuilder.AppendLine();
        dotnetApiCodeBuilder.AppendLine("GDExtensionClassLibraryPtr get_library() {");
        dotnetApiCodeBuilder.AppendLine("    return godot::internal::library;");
        dotnetApiCodeBuilder.AppendLine("}");
        
        // dotnetApiCodeBuilder.AppendLine("void init_interface_functions(struct interface_functions *interface_functions) {");

        var ignoredTypes = new List<string>
        {
            "char16_t",
            "char32_t",
        };

        
        
        string templateFile = Path.GetFullPath("res/GDExtension.template.xml");
        XDocument mappingDoc = XDocument.Load(templateFile);

        // Define the XML namespace
        XNamespace ns = "urn:SharpGen.Config";

        // Find the mapping element using the namespace
        XElement mappingElement = mappingDoc.Root?.Element(ns + "mapping");
        
        var functions = new Dictionary<string, CppType>();

        if (mappingElement == null)
        {
            // If the "mapping" element doesn't exist, you can create it and add content
            mappingElement = new XElement(ns + "mapping");
            mappingDoc.Root?.Add(mappingElement);
        }

        var functionMapElement = new XElement(ns + "map");
        functionMapElement.SetAttributeValue("function", "get_library");
        functionMapElement.SetAttributeValue("group", "GodotSharpGDExtension.GDExtensionInterface");
        functionMapElement.SetAttributeValue("dll", "\"fsharp.dll\"");
        mappingElement.Add(functionMapElement);
        
        
        // if (mappingNode is null)
        // {
        //     Console.Error.WriteLineAsync("Could not find config/mapping node");
        // }
        // mappingNode.RemoveAll();

        foreach (CppField field in interfaceFunctions)
        {
            string functionName = field.Name.Replace(gdextensionInterfacePrefix, "");

            CppType type = field.Type;
            
            CppFunctionType functionType = ExtractFunctionType(type);

            bool isIgnored = false;


            if (functionName.Contains("string_new_with_utf16_chars"))
            {
            }
            foreach (string ignoredType in ignoredTypes)
            {
                if (GetTypeString(functionType.ReturnType).Contains(ignoredType))
                {
                    isIgnored = true;
                    break;
                }
                
                if (!functionType.Parameters.Any(p => GetTypeString(p.Type).Contains(ignoredType)))
                {
                    continue;
                }

                isIgnored = true;
                break;
            }

            if (isIgnored) continue;
           
            if (IsFunction(functionType.ReturnType))
            {
                if (!functions.ContainsKey(functionType.ReturnType.GetDisplayName()))
                {
                    functions.Add(functionType.ReturnType.GetDisplayName(), functionType.ReturnType);
                }
                
            }

            var parameters = new List<string>();
            var parameterNames = new List<string>();
            
            foreach (CppParameter parameter in functionType.Parameters)
            {
                if (IsFunction(parameter.Type))
                {
                    if (!functions.ContainsKey(parameter.Type.GetDisplayName()))
                    {
                        functions.Add(parameter.Type.GetDisplayName(), parameter.Type);
                    }
                }
                
                parameters.Add(string.Format($"{GetTypeString(parameter.Type)}", parameter.Name));
                parameterNames.Add(parameter.Name);
            }

            var functionSignature =
                $"{functionType.ReturnType.GetDisplayName()} {functionName}({string.Join(", ", parameters)})";
            
            AddFunction(functionName, functionSignature, $"godot::internal::{field.Name}", functionType, parameterNames);
        }

        foreach (string functionKey in functions.Keys)
        {
            CppFunctionType functionType = ExtractFunctionType(functions[functionKey]);

            string functionName;
            if (functionKey == "void (*)(void*, uint32_t)*")
            {
                dotnetApiHeaderBuilder.AppendLine("typedef void (*NativeGroupTask)(void*, uint32_t);");
                functionName = "NativeGroupTask";
            }
            else if (functionKey == "void (*)(void*)*")
            {
                dotnetApiHeaderBuilder.AppendLine("typedef void (*NativeTask)(void*);");
                functionName = "NativeTask";
            }
            else
            {
                functionName = functionKey;
            }

            var parameters = new List<string>
            {
                "func",
            };
            var parameterNames = new List<string>();
            
            foreach ((CppParameter val, int index) parameter in functionType!.Parameters.Select((val, index) => (val, index)))
            {
                string parameterName = parameter.val.Name;
                if (string.IsNullOrWhiteSpace(parameterName))
                {
                    parameterName = $"arg{parameter.index}";
                }
                parameters.Add(string.Format($"{GetTypeString(parameter.val.Type)}", parameterName));
                parameterNames.Add(parameterName);
            }

            string callFunctionName = $"call_{functionName}";
            
            var functionSignature =
                $"{functionType.ReturnType.GetDisplayName()} {callFunctionName}({functionName} {string.Join(", ", parameters)})";

            AddFunction(callFunctionName, functionSignature, "func", functionType, parameterNames);
        }
        

        mappingDoc.Save(Path.GetFullPath(Path.Join(rootFolder,
            "GodotSharpGDExtension.CSharp/Mappings/GDExtension.xml")));
        
        
        // dotnetApiHeaderBuilder.AppendLine("};");
        // dotnetApiHeaderBuilder.AppendLine("void init_interface_functions(struct interface_functions *interface_functions);");
        //
        // dotnetApiCodeBuilder.AppendLine("}");


        await File.WriteAllTextAsync($"{rootFolder}GodotSharpGDExtension.Native/src/dotnet_api.h", dotnetApiHeaderBuilder.ToString().Replace("\t", "    "));
        await File.WriteAllTextAsync($"{rootFolder}GodotSharpGDExtension.Native/src/dotnet_api.cpp", dotnetApiCodeBuilder.ToString().Replace("\t", "    "));
        

        var pathToGenJson = Path.Combine(godotCppFolder, "gdextension", "extension_api.json");
        if (!File.Exists(pathToGenJson))
        {
            pathToGenJson = Path.Combine(Directory.GetCurrentDirectory(), "extension_api.json");
        }

        if (!File.Exists(pathToGenJson))
        {
            throw new Exception("Failed to find extension_api json");
        }

        var ginDirParent = Path.Combine(Directory.GetCurrentDirectory(), rootFolder, "GodotSharpGDExtension.CSharp");
        ginDirParent = Path.GetFullPath(ginDirParent);
        if (!Directory.Exists(ginDirParent))
        {
            ginDirParent = Path.Combine(Directory.GetCurrentDirectory(), "GodotSharpGDExtension.CSharp");
        }

        if (!Directory.Exists(ginDirParent))
        {
            ginDirParent = Path.Combine(Directory.GetCurrentDirectory(), "..", "GodotSharpGDExtension.CSharp");
        }

        ginDirParent = Path.GetFullPath(ginDirParent);
        if (!Directory.Exists(Path.Combine(ginDirParent, "Extensions")))
        {
            throw new Exception("Don't know where to put files");
        }

        var ginDir = Path.Combine(ginDirParent, "Generated");
        if (Directory.Exists(ginDir))
        {
            Directory.Delete(ginDir, true);
        }

        Directory.CreateDirectory(ginDir);

        // var processStartInfo = new ProcessStartInfo("dotnet",
        //     "run generate --config E:\\projekte\\LibGodotSharp\\GodotSharpGDExtension.CSharp\\CAstFfi\\config-generate-cs.json");
        //
        // processStartInfo.WorkingDirectory = "E:\\projekte\\c2cs\\src\\cs\\production\\C2CS.Tool";
        // // processStartInfo.RedirectStandardOutput = true;
        // // processStartInfo.RedirectStandardError = true;
        //
        // Process process = Process.Start(processStartInfo);
        //
        // if (process is null)
        // {
        //     Console.WriteLine("Could not run C2CS");
        //     return;
        // }
        //
        // await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));

        var docs = Path.Combine(Path.Combine(Environment.CurrentDirectory), "doc", "classes") + "/";
        var configName = "float_64";
        var api = Api.Create(pathToGenJson);
        var convert = new Convert(api, ginDir, docs, configName);
        convert.Emit();
        return;

        void AddFunction(string functionName, string functionSignature, string functionCall,
            CppFunctionType functionType,
            IEnumerable<string> parameterNames)
        {
            dotnetApiHeaderBuilder.AppendLine($"{functionSignature};");
            dotnetApiCodeBuilder.AppendLine($"{functionSignature} {{");
            string typeString = GetTypeString(functionType.ReturnType);

            var returnString = "";

            if (typeString != "void")
            {
                returnString = "return ";
            }

            dotnetApiCodeBuilder.AppendLine(
                $"\t{returnString}{functionCall}({string.Join(", ", parameterNames)});");
            dotnetApiCodeBuilder.AppendLine("}");
            
            var functionMapElement = new XElement(ns + "map");
            functionMapElement.SetAttributeValue("function", functionName);
            functionMapElement.SetAttributeValue("group", "GodotSharpGDExtension.GDExtensionInterface");
            functionMapElement.SetAttributeValue("dll", "\"fsharp.dll\"");
            mappingElement.Add(functionMapElement);
            
        }

        CppFunctionType ExtractFunctionType(CppType type)
        {
            while (type is not CppFunctionType)
            {
                switch (type)
                {
                    case CppFunctionType cppFunctionType:
                        type = cppFunctionType;
                        break;
                    case CppTypeWithElementType cppTypeWithElementType:
                        type = cppTypeWithElementType.ElementType;
                        continue;
                    case CppTypedef cppTypedef:
                        type = cppTypedef.ElementType;
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type));
                }
            }
            return (CppFunctionType)type;
        }
    }

    private static void CopyFilesRecursively(string sourcePath, string targetPath)
    {
        foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
        }

        foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }
    }

    /// <summary>
    /// Determines a text file's encoding by analyzing its byte order mark (BOM).
    /// Defaults to ASCII when detection of the text file's endianness fails.
    /// </summary>
    /// <param name="filename">The text file to analyze.</param>
    /// <returns>The detected encoding.</returns>
    public static Encoding GetEncoding(string filename)
    {
        // Read the BOM
        var bom = new byte[4];
        using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
        {
            file.Read(bom, 0, 4);
            file.Close();
        }

        // Analyze the BOM
#pragma warning disable SYSLIB0001
        if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
#pragma warning restore SYSLIB0001
        if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
        if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0) return Encoding.UTF32; //UTF-32LE
        if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
        if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
        if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff)
            return new UTF32Encoding(true, true); //UTF-32BE

        // We actually have no idea what the encoding is if we reach this point, so
        // you may wish to return null instead of defaulting to ASCII
        return Encoding.ASCII;
    }

    public static void ReplaceTextInFile(string file, string search, string replace)
    {
        var encode = GetEncoding(file);
        File.WriteAllText(file, File.ReadAllText(file).Replace(search, replace), encode);
    }

    public static void CopyFileWithDirectory(string sourceFilePath, string destFilePath)
    {
        if (!File.Exists(sourceFilePath))
        {
            Warn($"Did not find {sourceFilePath}");
            return;
        }

        // Ensure the directory exists
        string destDirectory = Path.GetDirectoryName(destFilePath);
        if (!Directory.Exists(destDirectory))
        {
            Directory.CreateDirectory(destDirectory);
        }

        // Copy the file
        File.Copy(sourceFilePath, destFilePath, true);
    }

    static bool IsFunction(CppType type)
    {
        while (true)
        {
            switch (type)
            {
                case CppFunctionType:
                    return true;
                case CppPrimitiveType:
                    return false;
                case CppClass:
                    return false;
                case CppEnum:
                    return false;
                case CppPointerType cppPointerType:
                    type = cppPointerType.ElementType;
                    continue;
                case CppArrayType:
                    return false;
                case CppQualifiedType:
                    return false;
                case CppReferenceType cppReferenceType:
                    type = cppReferenceType.ElementType;
                    continue;
                case CppTypedef cppTypedef:
                    type = cppTypedef.ElementType;
                    continue;
                case CppTypeWithElementType cppTypeWithElementType:
                    type = cppTypeWithElementType.ElementType;
                    continue;
                case CppUnexposedType:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }

            break;
        }
    }

    static string GetTypeString(CppType type, bool addNamePlaceholder = true, Func<StringBuilder, CppType, string, bool>? handleAction = null)
    {
        var typeStringBuilder = new StringBuilder();

        BuildTypeString(typeStringBuilder, type, addNamePlaceholder, handleAction: handleAction);

        return typeStringBuilder.ToString();
    }

    static void BuildTypeString(StringBuilder builder, CppType type,
        bool addNamePlaceholder = true,
        Func<StringBuilder, CppType, string, bool>? handleAction = null)
    {
        CppTypeKind? lastTypeKind = null;
        CppTypeQualifier? lastTypeQualifier = null;
        var postFix = "";
        var prefix = "";
        while (true)
        {
            //ICppMember

            postFix = lastTypeKind switch
            {
                CppTypeKind.Pointer => $"*{postFix}",
                CppTypeKind.Reference => $"&{postFix}",
                _ => postFix,
            };

            prefix = lastTypeQualifier switch
            {
                CppTypeQualifier.Const => "const ",
                CppTypeQualifier.Volatile => "volatile ",
                _ => prefix,
            };

            if (handleAction?.Invoke(builder, type, postFix) ?? false) break;

            
            switch (type)
            {
                case CppPrimitiveType cppPrimitiveType:
                    switch (cppPrimitiveType.Kind)
                    {
                        case CppPrimitiveKind.WChar:
                            builder.Append($"{prefix}wchar_t{postFix}");
                            if (addNamePlaceholder) builder.Append(" {0}");
                            break;
                        default:
                            builder.Append($"{prefix}{cppPrimitiveType.GetDisplayName()}{postFix}");
                            if (addNamePlaceholder) builder.Append(" {0}");
                            break;
                    }

                    break;
                case CppClass cppClass:
                    builder.Append($"{prefix}{cppClass.Name}{postFix}");
                    if (addNamePlaceholder) builder.Append(" {0}");
                    break;
                case CppEnum cppEnum:
                    builder.Append($"{prefix}{cppEnum.Name}{postFix}");
                    if (addNamePlaceholder) builder.Append(" {0}");
                    break;
                case CppPointerType cppPointerType:
                    type = cppPointerType.ElementType;
                    lastTypeKind = cppPointerType.TypeKind;
                    continue;
                case CppArrayType cppArrayType:
                    builder.Append($"{prefix}{cppArrayType.GetDisplayName()}");
                    if (addNamePlaceholder) builder.Append(" {0}");
                    break;
                case CppQualifiedType cppQualifiedType:
                    type = cppQualifiedType.ElementType;
                    lastTypeKind = null;
                    lastTypeQualifier = cppQualifiedType.Qualifier;
                    continue;
                case CppReferenceType cppReferenceType:
                    type = cppReferenceType.ElementType;
                    lastTypeKind = cppReferenceType.TypeKind;
                    continue;
                case CppTypedef cppTypedef:
                    builder.Append($"{prefix}{cppTypedef.GetDisplayName()}{postFix}");
                    if (addNamePlaceholder) builder.Append(" {0}");
                    break;
                case CppTypeWithElementType cppTypeWithElementType:
                    builder.Append($"{prefix}{cppTypeWithElementType.GetDisplayName()}");
                    if (addNamePlaceholder) builder.Append(" {0}");
                    break;
                case CppUnexposedType cppUnexposedType:
                    builder.Append($"{cppUnexposedType.Name}{postFix}");
                    if (addNamePlaceholder) builder.Append(" {0}");
                    break;
                case ICppMember iCppMember:
                    builder.Append($"{prefix}{iCppMember.Name}{postFix}");
                    if (addNamePlaceholder) builder.Append(" {0}");
                    break;
                case CppFunctionType functionType:
                        builder.Append($"{prefix}");
                        builder.Append($"{GetTypeString(functionType.ReturnType, false)}");
                        builder.Append("(*");
                        if (addNamePlaceholder) builder.Append("{0}");
                        builder.Append(')');
                        builder.Append('(');
                        builder.Append(string.Join(", ", functionType.Parameters.Select(p => GetTypeString(p.Type, false))));
                        builder.Append(')');
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }

            break;
        }
    }
}
