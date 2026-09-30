
using Personalhandboksassistent.File.Models;

namespace Personalhandboksassistent.Repositories
{
    /// <summary>
    /// Abstraction for a remote file storage backend.
    /// Supports CRUD operations: upload, download, read metadata, list, and delete.
    /// </summary>
    public interface IFileRepository
    {
        /// <summary>
        /// Retrieves metadata for a single stored file without downloading its content.
        /// </summary>
        /// <param name="path">The path or key of the file in storage.</param>
        /// <param name="ct">A cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="FileRef"/> with the file's metadata.</returns>
        Task<FileRef> GetInfoAsync(string path, CancellationToken ct);

        /// <summary>
        /// Retrieves a stored file's full content along with its metadata.
        /// The returned <see cref="FileData.Content"/> stream must be disposed by the caller.
        /// </summary>
        /// <param name="path">The path or key of the file in storage.</param>
        /// <param name="ct">A cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="FileData"/> containing the file's metadata and an open content stream.</returns>
        Task<FileData> GetAsync(string path, CancellationToken ct = default);

        /// <summary>
        /// Uploads a file to the storage backend, creating or overwriting the blob at the given path.
        /// </summary>
        /// <param name="path">The destination path or key in storage.</param>
        /// <param name="content">A stream containing the file content to upload.</param>
        /// <param name="contentType">The MIME content type to assign to the stored file.</param>
        /// <param name="ct">A cancellation token to cancel the operation.</param>
        Task UploadAsync(string path, Stream content, string contentType, CancellationToken ct = default);

        /// <summary>
        /// Deletes a stored file from the storage backend.
        /// </summary>
        /// <param name="path">The path or key of the file to delete.</param>
        /// <param name="ct">A cancellation token to cancel the operation.</param>
        Task DeleteAsync(string path, CancellationToken ct = default);

        /// <summary>
        /// Lists metadata for all stored files whose path starts with the given prefix.
        /// </summary>
        /// <param name="prefix">The path prefix to filter by (e.g. "documents/").</param>
        /// <param name="ct">A cancellation token to cancel the operation.</param>
        /// <returns>An async sequence of <see cref="FileRef"/> items matching the prefix.</returns>
        IAsyncEnumerable<FileRef> ListAsync(string prefix, CancellationToken ct = default);
    }
}
