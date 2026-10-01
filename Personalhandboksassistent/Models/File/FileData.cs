namespace Personalhandboksassistent.File.Models
{
    public sealed class FileData
    {
        public string Name { get; init; }

        public string ContentType { get; init; }

        public long Length { get; init; }

        public DateTimeOffset LastModified { get; init; }

        public string? ETag { get; init; }

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