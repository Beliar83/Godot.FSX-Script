#nullable enable
using System.Reflection;
using System.Runtime.Loader;

namespace FsxScriptAddon;

public class FsxScriptAssemblyLoadContext() : AssemblyLoadContext(true)
{
    protected override Assembly? Load(AssemblyName name)
    {
        return null;
    }
}
