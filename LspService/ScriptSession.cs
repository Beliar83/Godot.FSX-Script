using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Godot;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Range = Microsoft.VisualStudio.LanguageServer.Protocol.Range;

namespace LspService;

public class ScriptSession
{
    public static readonly List<ScriptSession> ActiveSessions = [];


    private readonly Script script;
    private Uri? lastScriptPath;

    public static ScriptSession? CreateScriptSession(Script script)
    {
        ScriptSession scriptSession = new(script);
        ActiveSessions.Add(scriptSession);
        return scriptSession;
    }

    protected ScriptSession(Script script)
    {
        this.script = script;
    }

    public string GetClassName()
    {
        throw new NotImplementedException();
    }

    public void Refresh()
    { }

    public void NotifyScriptClose()
    {
        if (lastScriptPath != null)
        {
            LspService.ScriptClosed(lastScriptPath);
        }
    }

    public void NotifyScriptChange()
    {
        if (!LspService.IsLspRunning) return;

        if (string.IsNullOrWhiteSpace(script.ResourcePath)) return;

        Uri scriptPath = new($"file://{ProjectSettings.Singleton.GlobalizePath(script.ResourcePath)}",
            UriKind.Absolute);
        if (lastScriptPath is not null && scriptPath != lastScriptPath)
        {
            LspService.ScriptClosed(lastScriptPath);
        }

        lastScriptPath = scriptPath;
        string scriptSourceCode = script.SourceCode;
        LspService.ScriptChanged(scriptPath, scriptSourceCode);
    }
}
