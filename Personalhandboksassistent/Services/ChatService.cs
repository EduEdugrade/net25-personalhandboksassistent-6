using Personalhandboksassistent.Models;

namespace Personalhandboksassistent.Services;

public class ChatService
{
    private readonly SearchService _search;
    private readonly AnswerService _answer;

    public ChatService(SearchService search, AnswerService answer)
    {
        _search = search;
        _answer = answer;
    }

    public async Task<ChatMessage> AskAsync(string question, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            return ChatMessage.NotFound("Skriv en fråga om personalhandboken.");
        }

        var chunks = await _search.SearchAsync(question, cancellationToken);

        if (chunks.Count == 0)
        {
            return ChatMessage.NotFound("Jag hittar inte det här i personalhandboken. Kontakta HR om du är osäker.");
        }

        var answer = await _answer.AnswerAsync(question, chunks, cancellationToken);

        if (answer.StartsWith("SAKNAS", StringComparison.OrdinalIgnoreCase))
        {
            return ChatMessage.NotFound("Jag hittar inte det här i personalhandboken. Kontakta HR om du är osäker.");
        }

        if (answer.StartsWith("UTANFOR", StringComparison.OrdinalIgnoreCase))
        {
            return ChatMessage.NotFound("Jag svarar bara på personalrelaterade frågor.");
        }

        return new ChatMessage
        {
            Role = ChatRoles.Assistant,
            Message = answer,
            IsFound = true,
            Sources = BuildSources(chunks)
        };
    }

    private static List<SourceReference> BuildSources(List<DocumentChunk> chunks)
        => chunks.Select((chunk, index) => new SourceReference
        {
            Number = index + 1,
            DocumentId = chunk.DocumentId,
            Title = chunk.Title ?? string.Empty,
            Page = PageOf(chunk),
            Snippet = SnippetOf(chunk.Content)
        }).ToList();

    private static string? PageOf(DocumentChunk chunk)
    {
        var marker = chunk.Id.LastIndexOf("_pages_", StringComparison.Ordinal);

        if (marker < 0 || !int.TryParse(chunk.Id[(marker + 7)..], out var page))
        {
            return null;
        }

        return (page + 1).ToString();
    }

    private static string SnippetOf(string text)
    {
        var firstLine = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();

        return firstLine is null or "" ? text[..Math.Min(text.Length, 200)] : firstLine;
    }
}

