namespace Personalhandboksassistent.Models;

public sealed class SourceReference
{
    public int Number { get; set; }
    public string DocumentId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Page { get; set; }
    public string Snippet { get; set; } = string.Empty;
}
