#if TOOLS
using Godot;
using Godot.Collections;
using Godot.FSharp;
using CodeEdit = Godot.CodeEdit;

namespace FsxScript;

[Tool]
[GlobalClass]
public partial class FsxScriptPlugin : EditorPlugin
{
    private static readonly Dictionary<string, FsxScriptSession> Sessions = new();

    public override void _EnterTree()
    {
        foreach (Script script in EditorInterface.Singleton.GetScriptEditor().GetOpenScripts())
        {
            if (!script.IsClass("FsxScript"))
            {
                continue;
            }

            string path = script.GetPath();
            FsxScriptSession session = new();
            Sessions[path] = session;
        }

        ScriptSession.SetBasePath(ProjectSettings.GlobalizePath("res://"));
        EditorInterface.Singleton.GetScriptEditor().EditorScriptChanged += OnEditorScriptChanged;
        EditorInterface.Singleton.GetScriptEditor().ScriptClose += OnScriptClose;
        AddExportPlugin(new FsxScriptExportPlugin());
    }

    private static void OnEditorScriptChanged(Script? script)
    {
        if (!script?.IsClass("FsxScript") ?? true)
        {
            return;
        }

        (EditorInterface.Singleton.GetScriptEditor().GetCurrentEditor().GetBaseEditor() as CodeEdit)!.IndentUseSpaces =
            true;
    }

    private static void OnScriptClose(Script script)
    {
        if (!script.IsClass("FsxScript"))
        {
            return;
        }

        string path = script.GetPath();
        Sessions.Remove(path);
    }

    /// <inheritdoc />
    public override void _Process(double _)
    {
        foreach (Script script in EditorInterface.Singleton.GetScriptEditor().GetOpenScripts())
        {
            if (!script.IsClass("FsxScript"))
            {
                continue;
            }

            string path = script.GetPath();
            if (Sessions.TryGetValue(path, out FsxScriptSession? session))
            {
                continue;
            }

            session = new FsxScriptSession();
            Sessions[path] = session;
        }
    }

    public override void _ExitTree()
    {
        Sessions.Clear();
    }

    private static FsxScriptSession GetOrCreateSession(string path)
    {
        if (Sessions.TryGetValue(path, out FsxScriptSession? session))
        {
            return session;
        }

        session = new FsxScriptSession();
        session.ScriptPath = path;
        Sessions[path] = session;
        return session;
    }
}
#endif
