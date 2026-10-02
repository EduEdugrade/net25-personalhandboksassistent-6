namespace Personalhandboksassistent.Services;

public static class DocumentRules
{
    private static readonly string[] AllowedExtensions = [".pdf", ".doc", ".docx", ".md", ".txt"];

    public const long MaxBytes = 20 * 1024 * 1024;

    public static bool IsAllowed(string fileName) => AllowedExtensions.Contains(Path.GetExtension(fileName).ToLowerInvariant());

    public static string ContentTypeOf(string fileName) => Path.GetExtension(fileName).ToLowerInvariant() switch
    {
        ".pdf" => "application/pdf",
        ".doc" => "application/msword",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ".md" => "text/markdown",
        ".txt" => "text/plain",
        _ => "application/octet-stream"
    };

    public static string AllowedList => string.Join(", ", AllowedExtensions);

    public static int MaxMegabytes => (int)(MaxBytes / 1024 / 1024);
}