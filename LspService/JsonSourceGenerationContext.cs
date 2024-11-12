using System.Text.Json.Serialization;
using Microsoft.VisualStudio.LanguageServer.Protocol;

namespace LspService;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(JsonRpcRequest<InitializeParams>))]
[JsonSerializable(typeof(JsonRpcRequest))]
[JsonSerializable(typeof(JsonRpcResponse))]
[JsonSerializable(typeof(TextDocumentContentChangeEvent))]
[JsonSerializable(typeof(JsonRpcRequest<JsonRpcResponse<InitializeResult>>))]
[JsonSerializable(typeof(JsonRpcRequest<DidOpenTextDocumentParams>))]
[JsonSerializable(typeof(JsonRpcRequest<DidChangeTextDocumentParams>))]
[JsonSerializable(typeof(JsonRpcRequest<DidCloseTextDocumentParams>))]
[JsonSerializable(typeof(JsonRpcRequest<WorkspaceLoadParams>))]
public partial class JsonSourceGenerationContext : JsonSerializerContext
{ }
