namespace Personalhandboksassistent.File.Models
{
    /// <summary>
    /// Full representation of a stored file, including both metadata and an open readable content stream.
    /// </summary>
    /// <param name="Name">The path or key identifying the file in storage.</param>
    /// <param name="ContentType">The MIME content type of the file (e.g. "application/pdf", "image/png").</param>
    /// <param name="Length">The size of the file in bytes.</param>
    /// <param name="LastModified">The timestamp of the last modification on the storage backend.</param>
    /// <param name="ETag">The HTTP ETag for optimistic concurrency control, or <c>null</c> if unavailable.</param>
    /// <param name="Content">An open, readable stream of the file's content. Must be disposed by the caller.</param>
    public sealed record FileData(
        string Name,
        string ContentType,
        long Length,
        DateTimeOffset LastModified,
        string? ETag,
        Stream Content);
}
