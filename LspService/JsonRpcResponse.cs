using System.Text.Json.Nodes;

namespace LspService;

public record JsonRpcRequest(string JsonRpc, string? Id, string Method);

public record JsonRpcRequest<T>(string JsonRpc, string? Id, string Method, T? Params)
    : JsonRpcRequest(JsonRpc, Id, Method)
{
    public JsonRpcRequest(string jsonRpc, string Method, T Params) : this(jsonRpc, null, Method, Params)
    { }
}

public record ResponseError(int Code, string Message, JsonObject? Data);

public record JsonRpcResponse(string JsonRpc, string? Id, ResponseError? Error);

public record JsonRpcResponse<T>(string JsonRpc, string? Id, T? Result, ResponseError? Error)
    : JsonRpcResponse(JsonRpc, Id, Error);
