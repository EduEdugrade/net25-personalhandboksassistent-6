using Personalhandboksassistent.File.Models;

namespace Personalhandboksassistent.Repositories
{
    public interface IFileRepository
    {
        Task<FileRef> GetInfoAsync(string path, CancellationToken ct);

        Task<FileData> GetAsync(string path, CancellationToken ct = default);

        Task UploadAsync(string path, Stream content, string contentType, CancellationToken ct = default);

        Task DeleteAsync(string path, CancellationToken ct = default);

        IAsyncEnumerable<FileRef> ListAsync(string prefix, CancellationToken ct = default);
    }
}
