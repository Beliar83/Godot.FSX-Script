using Microsoft.VisualStudio.LanguageServer.Protocol;

namespace LspService;

public record WorkspaceLoadParams(TextDocumentIdentifier[] TextDocuments);
