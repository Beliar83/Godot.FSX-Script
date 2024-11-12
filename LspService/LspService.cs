using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Godot;
using Microsoft.VisualStudio.LanguageServer.Protocol;

namespace LspService;

public class LspService
{
    private const string ProjectContent = """
                                          <Project Sdk="Microsoft.NET.Sdk">
                                              <PropertyGroup>
                                                  <TargetFramework>net8.0</TargetFramework>
                                              </PropertyGroup>
                                              <ItemGroup>
                                                  <PackageReference Include="Godot.Bindings" Version="4.3.0-nightly.24204.1" />
                                              </ItemGroup>    
                                          </Project>

                                          """;

    private const string GlobalJsonContent = """
                                             {
                                               "sdk": {
                                                 "version": "8.0.0",
                                                 "rollForward": "latestMinor"
                                               }
                                             }
                                             """;

    public static LspClient? lspClient;
    private static Process? lspProcess;

    private static readonly Dictionary<string, int> ScriptVersions = new();
    private static readonly Queue<Action> QueuedActions = new();

    [MemberNotNullWhen(true, nameof(lspClient))]
    [MemberNotNullWhen(true, nameof(lspProcess))]
    public static bool IsLspRunning => lspClient is not null || lspProcess is not null;

    [MemberNotNullWhen(true, nameof(lspClient))]
    [MemberNotNullWhen(true, nameof(lspProcess))]
    public static bool StartLsp()
    {
        if (IsLspRunning) return true;

        string projectName = ProjectSettings.Singleton.Get(new StringName("application/config/name")).AsString();

        string projectPath = ProjectSettings.Singleton.GlobalizePath($"res://{projectName}.fsproj");

        if (!File.Exists(projectPath))
        {
            GD.Print("Creating project file");
            File.WriteAllText(projectPath, ProjectContent);
        }

        string globalJsonPath = ProjectSettings.Singleton.GlobalizePath("res://global.json");
        if (!File.Exists(globalJsonPath))
        {
            GD.Print("Creating global.json file");
            File.WriteAllText(globalJsonPath, GlobalJsonContent);
        }

        GD.Print("Starting LSP Server");

        if (!File.Exists("./.config/dotnet-tools.json"))
        {
            Process.Start("dotnet", ["new", "tool-manifest"]).WaitForExit();
            Process.Start("dotnet", ["tool", "install", "--local", "fsautocomplete"]).WaitForExit();
        }

        Process.Start("dotnet", ["tool", "restore"]).WaitForExit();

        ProcessStartInfo info = new("dotnet", ["fsautocomplete", "-v"])
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
        };
        lspProcess = Process.Start(info);

        if (lspProcess == null || lspProcess.HasExited)
        {
            GD.PrintErr("Failed to LSP Server");
            return false;
        }

        lspProcess.Exited += (_, _) =>
        {
            GD.Print("Restarting LSP Server");
            while (!StartLsp())
            {
                Task.Delay(1000).Wait();
            }

            foreach (Action action in QueuedActions)
            {
                action.Invoke();
            }

            foreach (ScriptSession scriptSession in ScriptSession.ActiveSessions)
            {
                scriptSession.Refresh();
            }
        };

        lspClient = new LspClient(lspProcess.StandardInput, lspProcess.StandardOutput);
        InitializeResult? initResult = lspClient.Init();

        if (initResult is null)
        {
            lspProcess.Close();
            return false;
        }

        JsonRpcResponse? response = lspClient.Send("fsharp/workspaceLoad",
            new WorkspaceLoadParams([new TextDocumentIdentifier { Uri = new Uri($"file://{projectPath}") }]));

        if (response?.Error is not null)
        {
            GD.PrintErr($"Could not load project: {response.Error.Message} ({response.Error.Code})");
            if (response.Error.Data is not null)
            {
                GD.PrintErr($"Additional data: \n{response.Error.Data.ToJsonString()}");
            }

            lspProcess.Close();
            return false;
        }

        if (response is not null)
        {
            return true;
        }

        GD.PrintErr("Could not load project: Unknown error");
        lspProcess.Close();
        return false;
    }

    public static void ScriptOpened(Uri scriptPath, string scriptSourceCode)
    {
        if (ScriptVersions.ContainsKey(scriptPath.AbsolutePath)) return;

        if (!IsLspRunning)
        {
            GD.PrintErr("ScriptOpened: F# LSP Server is not running");
            QueuedActions.Enqueue(() => ScriptOpened(scriptPath, scriptSourceCode));
            return;
        }

        DidOpenTextDocumentParams openTextDocumentParams = new()
        {
            TextDocument = new TextDocumentItem
            {
                Uri = scriptPath,
                LanguageId = "fsharp",
                Version = 0,
                Text = scriptSourceCode,
            },
        };
        JsonRpcResponse? response = lspClient.Send("textDocument/didOpen", openTextDocumentParams);
        if (response?.Error is not null)
        {
            GD.PrintErr($"Could not parse script: {response.Error.Message}");
        }
        else if (response is null)
        {
            GD.PrintErr("Could not parse script: Unknown error");
        }
        else
        {
            ScriptVersions[scriptPath.AbsolutePath] = 0;
        }
    }

    public static void ScriptClosed(Uri scriptPath)
    {
        if (!IsLspRunning)
        {
            GD.PrintErr("ScriptOpened: F# LSP Server is not running");
            QueuedActions.Enqueue(() => ScriptClosed(scriptPath));
            return;
        }

        DidCloseTextDocumentParams closeTextDocumentParams = new()
        {
            TextDocument = new TextDocumentIdentifier { Uri = scriptPath },
        };


        JsonRpcResponse? response = lspClient.Send("textDocument/didClose", closeTextDocumentParams);
        if (response?.Error is not null)
        {
            GD.PrintErr($"Error when sending close notification: {response.Error.Message}");
        }
        else if (response is null)
        {
            GD.PrintErr("Error when sending close notification: Unknown error");
        }

        ScriptVersions.Remove(scriptPath.AbsolutePath);
    }

    public static void ScriptChanged(Uri scriptPath, string scriptSourceCode)
    {
        if (!IsLspRunning)
        {
            GD.PrintErr("ScriptChanged: F# LSP Server is not running");
            QueuedActions.Enqueue(() => ScriptChanged(scriptPath, scriptSourceCode));
            return;
        }

        if (!ScriptVersions.ContainsKey(scriptPath.AbsolutePath))
        {
            ScriptOpened(scriptPath, scriptSourceCode);
            return;
        }

        DidChangeTextDocumentParams changeTextDocumentParams = new()
        {
            TextDocument = new VersionedTextDocumentIdentifier
                { Version = ++ScriptVersions[scriptPath.AbsolutePath], Uri = scriptPath },
            ContentChanges = [new TextDocumentContentChangeEvent { Text = scriptSourceCode }],
        };
        JsonRpcResponse? response = lspClient.Send("textDocument/didChange", changeTextDocumentParams);
        if (response?.Error is not null)
        {
            GD.PrintErr($"Could not parse script: {response.Error.Message}");
        }
        else if (response is null)
        {
            GD.PrintErr("Could not parse script: Unknown error");
        }
    }
}
