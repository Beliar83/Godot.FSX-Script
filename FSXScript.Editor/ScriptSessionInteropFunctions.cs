using System.Runtime.InteropServices;

namespace FSXScript.Editor;

public unsafe struct ScriptSessionInteropFunctions(
    delegate* unmanaged<GCHandle> createScriptSession,
    delegate* unmanaged<GCHandle, ushort*, void> parseScript,
    delegate* unmanaged<GCHandle, ushort*> getClassName,
    delegate* unmanaged<GCHandle, void> destroyScriptSession)
{
    public delegate* unmanaged<GCHandle> createScriptSession = createScriptSession;
    public delegate* unmanaged<GCHandle, ushort*, void> parseScript = parseScript;
    public delegate* unmanaged<GCHandle, ushort*> getClassName = getClassName;
    public delegate* unmanaged<GCHandle, void> destroyScriptSession = destroyScriptSession;
}
