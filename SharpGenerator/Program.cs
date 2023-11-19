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
        if (args.Length < 4)
        {
            Console.WriteLine($"Usage: {args[0]} <Path for generated C# files> <Path for generated C++ files> <Path to godot-cpp>");
        }

        string folderForGeneratedCSharpFiles = args[1];
        string folderForGeneratedCppFiles = args[2];
        string godotCppFolder = args[3];
        
        Console.WriteLine($"Working Dir {Directory.GetCurrentDirectory()}");
        string rootFolder = "../../../../../";
        
        // string godotCppFolder = Path.GetFullPath($"{rootFolder}godot-cpp");
        Console.WriteLine($"GodotCpp Folder {godotCppFolder}");
        var cppParserOptions = new CppParserOptions();

        cppParserOptions.IncludeFolders.Add(Path.Join(godotCppFolder, "gdextension"));
        cppParserOptions.IncludeFolders.Add(Path.Join(godotCppFolder, "gen/include"));
        cppParserOptions.IncludeFolders.Add(Path.Join(godotCppFolder, "include"));

        string templateFile = Path.GetFullPath("res/GDExtension.template.xml");
        XDocument mappingDoc = XDocument.Load(templateFile);

        // Define the XML namespace
        XNamespace sharpGenNamespace = "urn:SharpGen.Config";

        // Find the mapping element using the namespace
        XElement mappingConfig = mappingDoc.Root!;
        XElement? mappingElement = mappingConfig.Element(sharpGenNamespace + "mapping");
        XElement? bindingElement = mappingConfig.Element(sharpGenNamespace + "bindings");
        XElement? namingElement = mappingConfig.Element(sharpGenNamespace + "naming");

        if (mappingElement == null)
        {
            // If the "mapping" element doesn't exist, you can create it and add content
            mappingElement = new XElement(sharpGenNamespace + "mapping");
            mappingConfig.Add(mappingElement);
        }

        if (bindingElement == null)
        {
            // If the "mapping" element doesn't exist, you can create it and add content
            bindingElement = new XElement(sharpGenNamespace + "bindings");
            mappingConfig.Add(bindingElement);
        }
        
        if (namingElement == null)
        {
            // If the "mapping" element doesn't exist, you can create it and add content
            namingElement = new XElement(sharpGenNamespace + "naming");
            mappingConfig.Add(namingElement);
        }

        foreach (string abbreviation in Fixer.Words.Keys)
        {
            var namingMapElement = new XElement(sharpGenNamespace + "short");
            namingMapElement.SetAttributeValue("name", abbreviation);
            namingMapElement.Value = abbreviation;
            namingElement.Add(namingMapElement);
        }
        
        {
            var functionMapElement = new XElement(sharpGenNamespace + "map");
            functionMapElement.SetAttributeValue("function", "add_extension_library");
            functionMapElement.SetAttributeValue("group", "GodotSharpGDExtension.GDExtensionInterface");
            functionMapElement.SetAttributeValue("dll", "\"godot_sharp_gdextension\"");
            mappingElement.Add(functionMapElement);
        }
        {
            var functionMapElement = new XElement(sharpGenNamespace + "map");
            functionMapElement.SetAttributeValue("function", "get_library");
            functionMapElement.SetAttributeValue("group", "GodotSharpGDExtension.GDExtensionInterface");
            functionMapElement.SetAttributeValue("dll", "\"godot_sharp_gdextension\"");
            mappingElement.Add(functionMapElement);
        }

        var pathToGenJson = Path.Combine(godotCppFolder, "gdextension", "extension_api.json");
        if (!File.Exists(pathToGenJson))
        {
            pathToGenJson = Path.Combine(Directory.GetCurrentDirectory(), "extension_api.json");
        }

        if (!File.Exists(pathToGenJson))
        {
            throw new Exception("Failed to find extension_api json");
        }
        
        // string generatedDir = Path.Combine(godotSharpGDExtensionCSharpDir, "Generated");
        // Directory.
        if (Directory.Exists(folderForGeneratedCSharpFiles))
        {
            Directory.Delete(folderForGeneratedCSharpFiles, true);
        }

        Directory.CreateDirectory(folderForGeneratedCSharpFiles);        
        
        if (Directory.Exists(folderForGeneratedCppFiles))
        {
            Directory.Delete(folderForGeneratedCppFiles, true);
        }

        Directory.CreateDirectory(folderForGeneratedCppFiles);
        Directory.CreateDirectory(Path.Join(folderForGeneratedCppFiles, "BuiltinClasses"));
        Directory.CreateDirectory(Path.Join(folderForGeneratedCppFiles, "UtilityFunctions"));
        Directory.CreateDirectory(Path.Join(folderForGeneratedCppFiles, "Classes"));

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

        StreamWriter godotDotnetSourceFile = File.CreateText($"{rootFolder}GodotSharpGDExtension.Native/src/generated/godot_dotnet.cpp");

        var convert = new Convert(api, folderForGeneratedCSharpFiles, folderForGeneratedCppFiles, docs, configName);
        convert.Emit();

        await godotDotnetSourceFile.WriteLineAsync($$"""
                                                //---------------------------------------------
                                                // This file is generated. Changes will be lost
                                                //---------------------------------------------
                                                
                                                #include "godot_dotnet.h"
                                                #include "godot_cpp/core/method_ptrcall.hpp"
                                                
                                                wchar_t* convert_string_to_dotnet(GDExtensionTypePtr string) {
                                                    const auto length = godot::internal::gdextension_interface_string_to_wide_chars(string, nullptr, 0);
                                                    const auto dotnet_string = new wchar_t[length];
                                                    godot::internal::gdextension_interface_string_to_wide_chars(string, dotnet_string, length);
                                                    return dotnet_string;
                                                }

                                                GDExtensionTypePtr convert_string_from_dotnet(const wchar_t* string) {
                                                    const auto new_string = new uint8_t[{{Convert.BuiltinClassSizes["String"]}}];
                                                    godot::internal::gdextension_interface_string_new_with_wide_chars(new_string, string);
                                                    return new_string;
                                                }
                                                
                                                GDExtensionBool convert_bool_from_dotnet(bool value) {
                                                    GDExtensionBool encoded;
                                                    godot::PtrToArg<bool>::encode(value, &encoded);
                                                    return encoded;
                                                }
                                                
                                                bool convert_bool_to_dotnet(GDExtensionBool value) {
                                                    return godot::PtrToArg<bool>::convert(&value);
                                                }
                                                """);
        
        godotDotnetSourceFile.Close();
        
        foreach (KeyValuePair<string,List<string>> classFunction in convert.BuiltinClassFunctions)
        {
            var includeElement = new XElement(sharpGenNamespace + "include");
            includeElement.SetAttributeValue("file", $"generated/BuiltinClasses/{classFunction.Key}.hpp");
            includeElement.SetAttributeValue("namespace", "GodotSharpGDExtension");
            includeElement.SetAttributeValue("attach", "true");
            mappingConfig.Add(includeElement);
            
            //mappingConfig
            foreach (string functionName in classFunction.Value)
            {
                var functionMapElement = new XElement(sharpGenNamespace + "map");
                functionMapElement.SetAttributeValue("function", functionName);
                functionMapElement.SetAttributeValue("group", "GodotSharpGDExtension.GDExtensionInterface");
                functionMapElement.SetAttributeValue("dll", "\"godot_sharp_gdextension\"");
                mappingElement.Add(functionMapElement);                
            }
        }
        
        mappingDoc.Save(Path.GetFullPath(Path.Join(rootFolder,
            "GodotSharpGDExtension.CSharp/Mappings/GDExtension.xml")));
        
        return;
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
