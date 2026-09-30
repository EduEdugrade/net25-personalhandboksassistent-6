namespace Personalhandboksassistent.File.Models
{
    /// <summary>
    /// Metadata-only representation of a stored file, without its content stream.
    /// </summary>
    public sealed class FileRef
    {
        /// <summary>The path or key identifying the file in storage.</summary>
        public string Name { get; init; }

        /// <summary>The MIME content type of the file (e.g. "application/pdf", "image/png").</summary>
        public string ContentType { get; init; }

        /// <summary>The size of the file in bytes.</summary>
        public long Length { get; init; }

        /// <summary>The timestamp of the last modification on the storage backend.</summary>
        public DateTimeOffset LastModified { get; init; }

        /// <summary>The HTTP ETag for optimistic concurrency control, or <c>null</c> if unavailable.</summary>
        public string? ETag { get; init; }

        public FileRef(string Name, string ContentType, long Length, DateTimeOffset LastModified, string? ETag)
        {
            this.Name = Name;
            this.ContentType = ContentType;
            this.Length = Length;
            this.LastModified = LastModified;
            this.ETag = ETag;
        }
    }
}