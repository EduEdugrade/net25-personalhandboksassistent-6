namespace Personalhandboksassistent.Models;

public sealed class DocumentChunk
{
    public string Id { get; set; } = string.Empty;
    public string DocumentId { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public int ChunkIndex { get; set; }
    public int? PageNumber { get; set; }
    public double? Score { get; set; }
}