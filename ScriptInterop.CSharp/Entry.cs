using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using FSharp.Compiler.Interactive;
using FSharp.Compiler.Syntax;
using FSXScriptCompiler;
using Microsoft.FSharp.Core;

namespace ScriptInterop.CSharp;

public static unsafe class Entry
{
    private static Dictionary<IntPtr, FSXScriptInstance> instances = new();

    private delegate IntPtr CreateFSXScriptInstanceDelegate(IntPtr pathPtr, IntPtr codePtr);
    
    private static readonly CreateFSXScriptInstanceDelegate CreateScriptInstance = (IntPtr pathPtr, IntPtr codePtr) =>
    {
        Console.Write("Creating dotnet instance");
        // while (!Debugger.IsAttached)
        // {
        //     
        // }

        var path = new GodotString(pathPtr, false).AsString();
        
        if (string.IsNullOrEmpty(path))
        {
            path = $"{Guid.NewGuid()}.fsx";
        }
        Console.WriteLine($"Path: {path}");
        var code = new GodotString(codePtr, false);
        Console.WriteLine($"Code: {code.AsString()}");
        // var path = new GodotString(path_ptr, false);
        // var path = new GodotString(path_ptr, false);
        // var code = new GodotString(code_ptr, false);
        
        var scriptName = new StringName(path);
        var scriptInstance = new FSXScriptInstance();
        scriptInstance.LoadSourceCode(path, code.AsString());
        IntPtr handle = StringName.getCPtr(scriptName).Handle;
        instances[handle] = scriptInstance;
        Console.WriteLine("Returning handle");
        return handle;
    };

    private delegate void CallMethodDelegate(IntPtr scriptPtr, IntPtr namePtr, IntPtr argsPtr, IntPtr instancePtr, out Variant returnVal);

    private static readonly CallMethodDelegate CallMethod =
        (IntPtr scriptPtr, IntPtr namePtr, IntPtr argsPtr, IntPtr instancePtr, out Variant returnVal) =>
        {
            var script = new StringName(scriptPtr, false);
            var name = new StringName(namePtr, false);
            var instance = new GodotObject(instancePtr, false);
            var args = new VariantVector(argsPtr, false);
            
            if (instances.TryGetValue(scriptPtr, out FSXScriptInstance? value))
            {
                returnVal = value.CallMethod(name.AsString(), args, instance);
            }
            else
            {
                Console.WriteLine($"Could not found script for {script.AsString()}");
                returnVal = new Variant();
            }
        };

    
    // private static FSXScript CreateScript()
    // {
    //     return new FSharpScriptDotnet();
    // }
    
    [UnmanagedCallersOnly]
    public static void Initialize()
    {
        Console.WriteLine("Initialize");
        NativeLibrary.SetDllImportResolver(
            Assembly.GetExecutingAssembly(), 
            NativeImportResolver);
        var test = new FSharpScriptDotnet("test.fsx");
        // foreach (FSharpMethodInfo methodInfo in test._get_methods())
        // {
        //     Console.WriteLine(methodInfo.Name.AsString());
        //     foreach (PropertyInfo propertyInfo in methodInfo.args)
        //     {
        //         Console.WriteLine(propertyInfo.class_name);   
        //         Console.WriteLine(propertyInfo.name);   
        //     }
        // }
        DotnetScriptInterop.print_script_info(test);
        var scriptSession = new ScriptSession();
        scriptSession.ParseScriptFromPath("test.fsx");
        FSharpOption<Shell.FsiValue> value = scriptSession.eval("(new GodotString(\"Test from FSX\")).AsString()");
        if (FSharpOption<Shell.FsiValue>.get_IsSome(value))
        {
            DotnetScriptInterop.test(value.Value.ReflectionValue as string);
        }
        
        var createDotnetInstance = new SWIGTYPE_p_f_godot__String_godot__String__p_godot__StringName(Marshal.GetFunctionPointerForDelegate(CreateScriptInstance), false);
        var callMethod = new SWIGTYPE_p_f_p_godot__StringName_godot__StringName_std__vector__godot__Variant___godot__Object_p_godot__Variant__void(Marshal.GetFunctionPointerForDelegate(CallMethod), false);
        DotnetScriptInterop.SetDotnetFunctions(createDotnetInstance, callMethod);
    }

    private static IntPtr NativeImportResolver(string name, Assembly assembly, DllImportSearchPath? path)
    {
        string libraryName;
        if (OperatingSystem.IsWindows())
        {
            libraryName = "fsharp.dll";
        }
        else if (OperatingSystem.IsLinux())
        {
            libraryName = "libfsharp.so";
        }
        else
        {
            throw new PlatformNotSupportedException();
        }
        
        return name == "fsharp" ? NativeLibrary.Load($"bin/{libraryName}") : IntPtr.Zero;
    }
}
