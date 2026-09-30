namespace Personalhandboksassistent.Models;

public class Chat
{
    public string Id { get; set; } = Guid.NewGuid().ToString("n");
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public List<ChatMessage> Messages { get; set; } = [];
}

public static class ChatRoles
{
    public const string User = "user";
    public const string Assistant = "assistant";
}

public class ChatMessage
{
    public string Role { get; set; } = ChatRoles.User;
    public string Message { get; set; } = string.Empty;
    public bool IsFound { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public List<SourceReference> Sources { get; set; } = [];
    public static ChatMessage NotFound(string message) => new()
    {
        Role = ChatRoles.Assistant,
        Message = message,
        IsFound = false,
    };
}

public record ChatRequest(string Question = "");