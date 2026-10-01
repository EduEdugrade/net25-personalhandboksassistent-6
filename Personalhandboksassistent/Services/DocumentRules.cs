namespace Personalhandboksassistent.Services;

public static class DocumentRules
{
    private static readonly string[] AllowedExtensions = [".pdf", ".doc", ".docx", ".md", ".txt"];

    public const long MaxBytes = 20 * 1024 * 1024;

    public static bool IsAllowed(string fileName) => AllowedExtensions.Contains(Path.GetExtension(fileName).ToLowerInvariant());

    public static string AllowedList => string.Join(", ", AllowedExtensions);

    public static int MaxMegabytes => (int)(MaxBytes / 1024 / 1024);
}