using Personalhandboksassistent.File.Models;
using DotNetEnv;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Runtime.CompilerServices;
using Azure.Storage;

namespace Personalhandboksassistent.Repositories
{
    /// <summary>
    /// Azure Blob Storage implementation of <see cref="IFileRepository"/>.
    /// Reads environment variables <c>AZURE_BLOB_CONNECTION_STRING</c> and <c>AZURE_BLOB_CONTAINER_NAME</c>
    /// to connect to the target storage account and container.
    /// </summary>
    public class AzBlobFileRepository : IFileRepository
    {
        private BlobContainerClient _containerClient;

        /// <summary>
        /// Initializes a new instance of <see cref="AzBlobFileRepository"/>.
        /// Loads <c>.env</c> if present, then reads <c>AZURE_BLOB_CONNECTION_STRING</c> and
        /// <c>AZURE_BLOB_CONTAINER_NAME</c> from environment variables to connect to the blob container.
        /// </summary>
        public AzBlobFileRepository()
        {
            Env.Load();

            var connectionString = Environment.GetEnvironmentVariable("AZURE_BLOB_CONNECTION_STRING");
            var containerName = Environment.GetEnvironmentVariable("AZURE_BLOB_CONTAINER_NAME");

            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(containerName))
                throw new ArgumentException("Connection string and/or container name is empty, did you forget to set the environment variables?");

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        }


        /// <summary>
        /// Retrieves metadata for a single stored file without downloading its content.
        /// </summary>
        /// <param name="path">The path or key of the file in storage.</param>
        /// <param name="ct">A cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="FileRef"/> with the file's metadata.</returns>
        public async Task<FileRef> GetInfoAsync(string path, CancellationToken ct)
        {
            var blobClient = _containerClient.GetBlobClient(path);
            var properties = await blobClient.GetPropertiesAsync();

            return new FileRef(
                  Name: path,
                  ContentType: properties.Value.ContentType,
                  Length: properties.Value.ContentLength,
                  LastModified: properties.Value.LastModified,
                  ETag: properties.Value.ETag.ToString());
        }

        /// <summary>
        /// Retrieves a stored file's full content along with its metadata.
        /// The returned <see cref="FileData.Content"/> stream must be disposed by the caller.
        /// </summary>
        /// <remarks>
        ///  <example>
        /// Fullfilling a download request:
        /// <code>
        /// {
        ///     var file = await _repo.GetAsync(path, ct);
        ///     return File(file.Content,
        ///                 file.ContentType,
        ///                 file.Name,
        ///                 enableRangeProcessing: true);
        /// }
        /// </code>
        /// </example>
        /// </remarks>
        /// <param name="path">The path or key of the file in storage.</param>
        /// <param name="ct">A cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="FileData"/> containing the file's metadata and an open content stream.</returns>
        public async Task<FileData> GetAsync(string path, CancellationToken ct = default)
        {
            var blobClient = _containerClient.GetBlobClient(path);
            var properties = await blobClient.GetPropertiesAsync();
            Stream stream = await blobClient.OpenReadAsync(cancellationToken: ct);

            return new FileData(
                  Name: path,
                  ContentType: properties.Value.ContentType,
                  Length: properties.Value.ContentLength,
                  LastModified: properties.Value.LastModified,
                  ETag: properties.Value.ETag.ToString(),
                  Content: stream);
        }

        /// <summary>
        /// Deletes a stored file from the Azure Blob container.
        /// </summary>
        /// <param name="path">The path or key of the file to delete.</param>
        /// <param name="ct">A cancellation token to cancel the operation.</param>
        public async Task DeleteAsync(string path, CancellationToken ct = default)
        {
            await _containerClient.DeleteBlobAsync(path);
        }

        /// <summary>
        /// Lists metadata for all stored files whose path starts with the given prefix.
        /// Results are streamed as an async sequence.
        /// </summary>
        /// <param name="prefix">The path prefix to filter by (e.g. "documents/").</param>
        /// <param name="ct">A cancellation token to cancel the operation.</param>
        /// <returns>An async sequence of <see cref="FileRef"/> items matching the prefix.</returns>
        public async IAsyncEnumerable<FileRef> ListAsync(string prefix, [EnumeratorCancellation] CancellationToken ct = default)
        {
            await foreach (var item in _containerClient.GetBlobsAsync(BlobTraits.None, BlobStates.None, prefix, ct))
            {
                yield return new FileRef(
                    item.Name,
                    item.Properties.ContentType,
                    item.Properties.ContentLength ?? 0,
                    item.Properties.LastModified ?? default,
                    item.Properties.ETag?.ToString());
            }
        }


        /// <summary>
        /// Uploads a file to the Azure Blob container, creating it or overwriting an existing blob at the given path.
        /// </summary>
        /// <param name="path">The destination path or key in storage.</param>
        /// <param name="content">A stream containing the file content to upload.</param>
        /// <param name="contentType">The MIME content type to assign to the stored file.</param>
        /// <param name="ct">A cancellation token to cancel the operation.</param>
        public async Task UploadAsync(string path, Stream content, string contentType, CancellationToken ct = default)
        {
            BlobClient blobClient = _containerClient.GetBlobClient(path);

            await blobClient.UploadAsync(content, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
                TransferOptions = new StorageTransferOptions
                {
                    MaximumConcurrency = 8,
                    MaximumTransferSize = 16 * 1024 * 1024
                }
            });
        }



    }
}
