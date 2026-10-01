namespace Personalhandboksassistent.File.Models
{
    public sealed class FileRef
    {
        public string Name { get; init; }

        public string ContentType { get; init; }

        public long Length { get; init; }

        public DateTimeOffset LastModified { get; init; }

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