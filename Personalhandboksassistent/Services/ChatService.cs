using Personalhandboksassistent.Models;
using Personalhandboksassistent.Repositories;

namespace Personalhandboksassistent.Services;

public class ChatService(SearchService search, AnswerService answer, IChatRepository chats)
{
    public async Task<ChatMessage> AskAsync(string user, string question, CancellationToken cancellationToken = default)
    {
        var chat = await chats.GetAsync(user, cancellationToken);
        var reply = await AskAsync(question, chat.Messages, cancellationToken);

        chat.Messages.Add(new ChatMessage { Role = ChatRoles.User, Message = question });
        chat.Messages.Add(reply);
        chat.UpdatedAt = DateTimeOffset.UtcNow;

        await chats.SaveAsync(user, chat, cancellationToken);

        return reply;
    }

    public Task ClearAsync(string user, CancellationToken cancellationToken = default) => chats.ClearAsync(user, cancellationToken);

    private async Task<ChatMessage> AskAsync(string question, List<ChatMessage> history, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            return ChatMessage.NotFound("Skriv en fråga om personalhandboken.");
        }

        var chunks = await search.SearchAsync(question, cancellationToken);

        if (chunks.Count == 0)
        {
            return ChatMessage.NotFound("Jag hittar inte det här i personalhandboken. Kontakta HR om du är osäker.");
        }

        var reply = await answer.AnswerAsync(question, chunks, history, cancellationToken);

        if (reply.StartsWith("SAKNAS", StringComparison.OrdinalIgnoreCase))
        {
            return ChatMessage.NotFound("Jag hittar inte det här i personalhandboken. Kontakta HR om du är osäker.");
        }

        if (reply.StartsWith("UTANFOR", StringComparison.OrdinalIgnoreCase))
        {
            return ChatMessage.NotFound("Jag svarar bara på personalrelaterade frågor.");
        }

        return new ChatMessage
        {
            Role = ChatRoles.Assistant,
            Message = reply,
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

