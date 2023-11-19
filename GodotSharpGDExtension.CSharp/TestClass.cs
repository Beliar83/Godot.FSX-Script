namespace GodotSharpGDExtension;

public class TestClass
{
    public void VarargTest(params object[] parameters)
    {
        var varargs = new Array();
        foreach (object parameter in parameters)
        {
            // if (parameter is GodotObject godotObject)
            // {
            //     godotObject.InternalPointer
            // }
        }
        // varargs.InternalPointer
    }    
}
