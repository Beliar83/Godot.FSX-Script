using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace LspService;

public partial class ScriptSession : GodotObject, IScriptSession
{
    public static readonly List<ScriptSession> ActiveSessions = [];

    private Uri? lastScriptPath;
    private string? path;
    private string? code;

    public ScriptSession()
    {
        ActiveSessions.Add(this);
    }

    /// <inheritdoc />
    public void SetPath(string path)
    {
        this.path = path;
    }

    /// <inheritdoc />
    public void SetCode(string code)
    {
        this.code = code;
    }

    public string GetClassName()
    {
        throw new NotImplementedException();
    }

    public async Task NotifyScriptCloseAsync()
    {
        if (lastScriptPath != null)
        {
            await LspService.ScriptClosed(lastScriptPath);
        }
    }

    public async Task NotifyScriptChangeAsync()
    {
        if (!LspService.IsLspRunning) return;

        if (string.IsNullOrWhiteSpace(path)) return;

        Uri scriptPath = new($"file://{ProjectSettings.Singleton.GlobalizePath(path)}",
            UriKind.Absolute);
        if (lastScriptPath is not null && scriptPath != lastScriptPath)
        {
            await LspService.ScriptClosed(lastScriptPath);
        }

        lastScriptPath = scriptPath;
        string scriptSourceCode = code ?? "";
        await LspService.ScriptChanged(scriptPath, scriptSourceCode);
    }
}
