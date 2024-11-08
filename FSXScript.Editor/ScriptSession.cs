using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using FSXScriptLanguage;
using Godot;

namespace FSXScript.Editor;

public class ScriptSession(GCHandle scriptSessionHandle) : IScriptSession, IDisposable
{
    public static ScriptSessionInteropFunctions? InteropFunctions { get; internal set; }

    public static unsafe ScriptSession? CreateScriptSession()
    {
        if (InteropFunctions is not null)
        {
            return new ScriptSession(InteropFunctions.Value.createScriptSession());
        }

        GD.PrintErr("ScriptSession.CreateScriptSession(): InteropFunctions is not initialized");
        return null;
    }

    /// <inheritdoc />
    public unsafe string GetClassName()
    {
        return Utf16StringMarshaller.ConvertToManaged(InteropFunctions!.Value.getClassName(scriptSessionHandle)) ?? "";
    }

    /// <inheritdoc />
    public unsafe void ParseScript(string script)
    {
        ushort* scriptPointer = Utf16StringMarshaller.ConvertToUnmanaged(script);
        InteropFunctions!.Value.parseScript(scriptSessionHandle, scriptPointer);
    }

    /// <inheritdoc />
    public unsafe void Dispose()
    {
        InteropFunctions!.Value.destroyScriptSession(scriptSessionHandle);
        GC.SuppressFinalize(this);
    }
}
