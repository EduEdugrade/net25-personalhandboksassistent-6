using Personalhandboksassistent.Models;
using Personalhandboksassistent.Repositories;

namespace Personalhandboksassistent.Services;

public class DocumentService(IFileRepository files, IndexerService indexer)
{
    public async Task<List<DocumentInfo>> ListAsync(CancellationToken cancellationToken = default)
    {
        var documents = new List<DocumentInfo>();

        await foreach (var file in files.ListAsync("", cancellationToken))
        {
            documents.Add(new DocumentInfo
            {
                Name = file.Name,
                Size = file.Length,
                ContentType = file.ContentType,
                LastModified = file.LastModified
            });
        }

        return documents.OrderBy(document => document.Name).ToList();
    }

    public async Task<int> UploadAsync(string fileName, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        await files.UploadAsync(fileName, content, contentType, cancellationToken);

        var removed = await indexer.DeleteChunksAsync(fileName, cancellationToken);

        await indexer.RunAsync(cancellationToken);

        return removed;
    }

    public async Task<int> DeleteAsync(string fileName, CancellationToken cancellationToken = default)
    {
        await files.DeleteAsync(fileName, cancellationToken);

        return await indexer.DeleteChunksAsync(fileName, cancellationToken);
    }

    public async Task<int> CleanOrphansAsync(CancellationToken cancellationToken = default)
    {
        var filesInContainer = (await ListAsync(cancellationToken)).Select(document => document.Name).ToList();

        return await indexer.CleanOrphansAsync(filesInContainer, cancellationToken);
    }
}
