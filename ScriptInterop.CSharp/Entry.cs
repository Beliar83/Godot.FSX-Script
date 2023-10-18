using System.Reflection;
using System.Runtime.InteropServices;

namespace ScriptInterop.CSharp;

public class Entry
{
    [UnmanagedCallersOnly]
    public static void Initialize()
    {
        NativeLibrary.SetDllImportResolver(
            Assembly.GetExecutingAssembly(), 
            NativeImportResolver);
        var test = new Test();
        // foreach (FSharpMethodInfo methodInfo in test._getMethods())
        // {
        //     Console.WriteLine(methodInfo.Name.AsString());
        //     foreach (PropertyInfo propertyInfo in methodInfo.args)
        //     {
        //         Console.WriteLine(propertyInfo.class_name);   
        //         Console.WriteLine(propertyInfo.name);   
        //     }
        // }
        DotnetScriptInterop.print_script_info(test);
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
