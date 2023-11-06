using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Generators;

public static class Entry
{

    public static void Execute(GeneratorExecutionContext context, List<Register.Data> classes)
    {
        var registrations = "";
        var editorRegistrations = "";
        var unregistrations = "";

        var classNames = new List<string>();


        var assemblyName = context.Compilation.AssemblyName ?? "NoName";
        var className = $"{assemblyName.Replace('.', '_')}ExtensionEntry";
        foreach (Register.Data n in classes)
        {
            switch (n.level)
            {
                case Register.Level.Scene:
                    registrations += $"{n.@namespace}.{n.name}.Register();\n\t\t\t";
                    break;
                case Register.Level.Editor:
                    editorRegistrations += $"{n.@namespace}.{n.name}.Register();\n\t\t\t";
                    break;
            }
            
            unregistrations = $"GDExtensionInterface.ClassdbUnregisterExtensionClass({className}.Library, {n.@namespace}.{n.name}.__godot_name.internalPointer);\n\t\t\t" + unregistrations;
        }
        Debug.WriteLine($"Building Entry {registrations}");
        classNames.Add(className);
        var source = $$"""
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GodotSharpGDExtension;
namespace {{assemblyName}};

public unsafe static class {{className}} {

    static public IntPtr Library { get; private set; }
    
    [UnmanagedCallersOnly]
    public static void Bind()
    {
        Library = GDExtensionInterface.GetLibrary();
    }

    [UnmanagedCallersOnly]
    public static void Initialize(GDExtensionInitializationLevel level) 
    {
        switch (level)
        {
            case GDExtensionInitializationLevel.GdextensionInitializationCore:
                break;
            case GDExtensionInitializationLevel.GdextensionInitializationServers:
                break;
            case GDExtensionInitializationLevel.GdextensionInitializationScene:
                // Register all not yet implicitly registered godot classes
                Register.RegisterBuiltin();
                Register.RegisterUtility();
                Register.RegisterCore();
                Register.RegisterServers();
                Register.RegisterScene();
                {{registrations}}
                break;
            case GDExtensionInitializationLevel.GdextensionInitializationEditor:
                // Register all not yet implicitly registered godot editor classes
                Register.RegisterEditor();
                {{editorRegistrations}}
                break;
        }
    }

    [UnmanagedCallersOnly]
    public static void Uninitialize(GDExtensionInitializationLevel level)
    {
        switch (level)
        {
            case GDExtensionInitializationLevel.GdextensionInitializationCore:
                break;
            case GDExtensionInitializationLevel.GdextensionInitializationServers:
                break;
            case GDExtensionInitializationLevel.GdextensionInitializationScene:
                {{unregistrations}}
                break;
            case GDExtensionInitializationLevel.GdextensionInitializationEditor:
                break;
        }
    }
}
""";
        context.AddSource("ExtensionEntry.gen.cs", source);
    }
}