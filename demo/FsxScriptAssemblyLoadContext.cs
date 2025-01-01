using System.Reflection;
using System.Runtime.Loader;

namespace FsxScript;

public class FsxScriptAssemblyLoadContext() : AssemblyLoadContext(true)
{
    protected override Assembly? Load(AssemblyName name)
    {
        return null;
    }
}
