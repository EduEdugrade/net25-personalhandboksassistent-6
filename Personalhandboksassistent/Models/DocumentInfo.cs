namespace Personalhandboksassistent.Models;

public class DocumentInfo
{
    public string Name { get; set; } = string.Empty;
    public long Size { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTimeOffset LastModified { get; set; }
    public string Type => Path.GetExtension(Name).TrimStart('.').ToUpperInvariant();
}
