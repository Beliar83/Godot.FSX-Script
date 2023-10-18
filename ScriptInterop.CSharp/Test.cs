namespace ScriptInterop.CSharp;

public class Test : FSharpScript
{
    /// <inheritdoc />
    public override MethodInfoVector _getMethods()
    {
        var methodInfoVector = new MethodInfoVector();
        var fSharpMethodInfo = new FSharpMethodInfo { Name = new GodotString("TestMethod")};
        fSharpMethodInfo.args.Add(new PropertyInfo(Variant.Type.INT, new StringName("Test")));
        methodInfoVector.Add(fSharpMethodInfo);
        return methodInfoVector;
    }
}
