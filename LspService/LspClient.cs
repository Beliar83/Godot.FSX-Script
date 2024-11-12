using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using Microsoft.VisualStudio.LanguageServer.Protocol;

namespace LspService;

public class LspClient(StreamWriter inputStream, StreamReader outputStream)
{
    private readonly JsonSerializerOptions jsonOptions = new()
        { TypeInfoResolver = new JsonSourceGenerationContext(), PropertyNamingPolicy = new LowerCaseNamingPolicy(), DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull};

    private string version = "2.0";

    private T? ReadResponseFromOutputStream<T>()
        where T : JsonRpcResponse
    {
        string? response = outputStream.ReadLine();
        outputStream.ReadLine();
        if (response is null || !response.StartsWith("Content-Length"))
        {
            return null;
        }

        int length = int.Parse(response.Split(':')[1].Trim());
        char[] buffer = new char[length];
        outputStream.Read(buffer, 0, length);

        return JsonSerializer.Deserialize<T>(buffer, jsonOptions);
    }

    private void SendRequest(JsonRpcRequest jsonRpcRequest)
    {
        string json = JsonSerializer.Serialize(jsonRpcRequest, jsonOptions);
        WriteToInputStream(json);
    }

    private string CreateRequest<TRequest>(string method, TRequest? request)
    {
        Guid id = Guid.NewGuid();
        JsonRpcRequest<TRequest> jsonRpcRequest = new(version, id.ToString(), method, request);
        return JsonSerializer.Serialize(jsonRpcRequest, jsonOptions);
    }

    private string CreateRequest(string method)
    {
        Guid id = Guid.NewGuid();
        JsonRpcRequest jsonRpcRequest = new(version, id.ToString(), method);
        return JsonSerializer.Serialize(jsonRpcRequest, jsonOptions);
    }

    private void WriteToInputStream(string json)
    {
        inputStream.WriteLine($"Content-Length: {json.Length}");
        inputStream.WriteLine();
        inputStream.Write(json);
        inputStream.Flush();
    }

    public JsonRpcResponse<TResponse>? SendAndGet<TRequest, TResponse>(string method, TRequest? request)
    {
        string jsonRpcRequest = CreateRequest(method, request);
        WriteToInputStream(jsonRpcRequest);
        return ReadResponseFromOutputStream<JsonRpcResponse<TResponse>>();
    }

    public JsonRpcResponse? SendAndGet<TRequest>(string method, TRequest? request)
    {
        string jsonRpcRequest = CreateRequest(method, request);
        WriteToInputStream(jsonRpcRequest);
        return ReadResponseFromOutputStream<JsonRpcResponse>();
    }

    public JsonRpcResponse<TResponse>? SendAndGet<TResponse>(string method)
    {
        string jsonRpcRequest = CreateRequest(method);
        WriteToInputStream(jsonRpcRequest);
        return ReadResponseFromOutputStream<JsonRpcResponse<TResponse>>();
    }

    public JsonRpcResponse? Send<T>(string method, T request)
    {
        string jsonRpcRequest = CreateRequest(method, request);
        WriteToInputStream(jsonRpcRequest);
        return ReadResponseFromOutputStream<JsonRpcResponse>();
    }

    public JsonRpcResponse? Send(string method)
    {
        string jsonRpcRequest = CreateRequest(method);
        WriteToInputStream(jsonRpcRequest);
        return ReadResponseFromOutputStream<JsonRpcResponse>();
    }

    public InitializeResult? Init()
    {
        JsonRpcResponse<InitializeResult>? result = SendAndGet<InitializeParams, InitializeResult>("initialize",
            new InitializeParams
            {
                Capabilities = new ClientCapabilities(), ProcessId = Process.GetCurrentProcess().Id,
                Trace = TraceSetting.Verbose
            });
        if (result == null) return null;

        if (result.Error != null)
        {
            GD.PrintErr($"Got error from LSP Server: {result.Error.Message} ({result.Error.Code})");
            if (result.Error.Data is not null)
            {
                GD.PrintErr($"Additional data: \n{result.Error.Data.ToJsonString()}");
            }

            return null;
        }

        version = result.JsonRpc;
        return result.Result;
    }
}
