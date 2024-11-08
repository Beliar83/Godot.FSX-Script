using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using FSXScriptCompiler;

namespace FSXScriptInterpreter.Interop;

public static unsafe class Interop
{
    [UnmanagedCallersOnly]
    public static GCHandle CreateScriptSession()
    {
        ScriptSession scriptSession = new();
        return GCHandle.Alloc(scriptSession, GCHandleType.Normal);
    }

    [UnmanagedCallersOnly]
    public static void ParseScript(GCHandle scriptSession, ushort* scriptPointer)
    {
        string? script = Utf16StringMarshaller.ConvertToManaged(scriptPointer);
        ((ScriptSession)scriptSession.Target!).ParseScript(script ?? string.Empty);
    }

    [UnmanagedCallersOnly]
    public static ushort* GetClassName(GCHandle scriptSession)
    {
        char className = Utf16StringMarshaller.GetPinnableReference(
            ((ScriptSession)scriptSession.Target!).ClassName);
        return (ushort*)&className;
    }

    [UnmanagedCallersOnly]
    public static void DestroyScriptSession(GCHandle scriptSession)
    {
        scriptSession.Free();
    }
}
