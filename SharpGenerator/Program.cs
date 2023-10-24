using System.Diagnostics;

namespace SharpGenerator;

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
        const string rootFolder = "../../../../../";
        
        string generatedDir = GetGeneratedDir(rootFolder, "GodotSharpGDExtension.CSharp");

        if (Directory.Exists(generatedDir))
        {
            Directory.Delete(generatedDir, true);
        }

        Directory.CreateDirectory(generatedDir);

        string swigSrcFolder =  Path.GetFullPath(Path.Join(rootFolder, "GodotSharpGDExtension/src/swig"));
        const string swigBaseArguments = "-csharp -c++ -dllimport bin/fsharp -outcurrentdir";
        var swigProcessStartInfo = new ProcessStartInfo("e:/swigwin/swigwin-4.1.1/swig",
            $"{swigBaseArguments} -outdir {generatedDir} {Path.GetFullPath(Path.Join(rootFolder, "GodotSharpGDExtension/swig_modules/godot_sharp_swig.i"))}")
        {
            WorkingDirectory = swigSrcFolder,
        };

        Process process = Process.Start(swigProcessStartInfo);

        if (process is null)
        {
            Console.WriteLine("Could not run swig");
            return;
        }
        
        await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
        
        generatedDir = GetGeneratedDir(rootFolder, "ScriptInterop.Interface");

        if (Directory.Exists(generatedDir))
        {
            Directory.Delete(generatedDir, true);
        }
        
        Directory.CreateDirectory(generatedDir);
        
        swigProcessStartInfo.Arguments = $"{swigBaseArguments} -outdir {generatedDir} {Path.GetFullPath(Path.Join(rootFolder, "GodotSharpGDExtension/swig_modules/script_interop.i"))}";

        process = Process.Start(swigProcessStartInfo);
        
        if (process is null)
        {
            Console.WriteLine("Could not run swig");
            return;
        }
        
        await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
    }

    private static string GetGeneratedDir(string rootFolder, string godotsharpgdextensionCsharp)
    {
        string ginDirParent = Path.Combine(Directory.GetCurrentDirectory(), rootFolder, godotsharpgdextensionCsharp);
        ginDirParent = Path.GetFullPath(ginDirParent);
        if (!Directory.Exists(ginDirParent))
        {
            ginDirParent = Path.Combine(Directory.GetCurrentDirectory(), godotsharpgdextensionCsharp);
        }

        if (!Directory.Exists(ginDirParent))
        {
            ginDirParent = Path.Combine(Directory.GetCurrentDirectory(), "..", godotsharpgdextensionCsharp);
        }

        ginDirParent = Path.GetFullPath(ginDirParent);

        string generatedDir = Path.Combine(ginDirParent, "Generated");
        return generatedDir;
    }
}
