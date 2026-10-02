using Personalhandboksassistent.File.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Runtime.CompilerServices;
using Azure.Storage;

namespace Personalhandboksassistent.Repositories
{
    public class AzBlobFileRepository : IFileRepository
    {
        private BlobContainerClient _containerClient;

        public AzBlobFileRepository()
        {
            var connectionString = Environment.GetEnvironmentVariable("AZURE_BLOB_CONNECTION_STRING");
            var containerName = Environment.GetEnvironmentVariable("AZURE_BLOB_CONTAINER_NAME");

            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(containerName))
                throw new ArgumentException("Connection string and/or container name is empty, did you forget to set the environment variables?");

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        }

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

        public async Task DeleteAsync(string path, CancellationToken ct = default)
        {
            await _containerClient.DeleteBlobAsync(path);
        }

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
