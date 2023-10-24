using System.Reflection;
using FSXScriptCompiler;

namespace ScriptInterop.CSharp;

public class FSharpScriptDotnet : FSXScript
{
    private ScriptSession scriptSession;

    public FSharpScriptDotnet()
    {
        scriptSession = new ScriptSession();
    }

    public FSharpScriptDotnet(string script) : this()
    {
        scriptSession.ParseScriptFromPath(script);
    }
    
    /// <inheritdoc />
    public MethodInfoVector GetMethods()
    {
        return scriptSession.GetMethods();
    }

    /// <inheritdoc />
    public Variant CallMethod(StringName name, VariantVector args, GodotObject instance)
    {
        Console.WriteLine($"Calling {name.AsString()}");
        scriptSession.Call(name.AsString(), instance);
        return new Variant();
    }

    /// <inheritdoc />
    public void LoadSourceCode(GodotString path, GodotString code)
    {
        scriptSession.ParseScriptFromCode(path.AsString(), code.AsString());
    }
}
