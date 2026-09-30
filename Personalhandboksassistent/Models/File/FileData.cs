namespace Personalhandboksassistent.File.Models
{
    /// <summary>
    /// Full representation of a stored file, including both metadata and an open readable content stream.
    /// The <see cref="Content"/> stream must be disposed by the caller.
    /// </summary>
    public sealed class FileData
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

        /// <summary>An open, readable stream of the file's content. Must be disposed by the caller.</summary>
        public Stream Content { get; init; }

        public FileData(string Name, string ContentType, long Length, DateTimeOffset LastModified, string? ETag, Stream Content)
        {
            this.Name = Name;
            this.ContentType = ContentType;
            this.Length = Length;
            this.LastModified = LastModified;
            this.ETag = ETag;
            this.Content = Content;
        }
    }
}