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

    public async Task<UploadResult> UploadAsync(string fileName, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        await files.UploadAsync(fileName, content, contentType, cancellationToken);

        var removed = await indexer.DeleteChunksAsync(fileName, cancellationToken);

        await indexer.RunAsync(cancellationToken);

        var timeout = int.TryParse(Environment.GetEnvironmentVariable("INDEXER_WAIT_TIMEOUT_SECONDS"), out var seconds) ? seconds : 90;
        var indexed = await indexer.WaitUntilIndexedAsync(fileName, timeout, cancellationToken);

        return new UploadResult(removed, indexed);
    }

    public async Task<int> DeleteAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var chunksRemoved = await indexer.DeleteChunksAsync(fileName, cancellationToken);

        await files.DeleteAsync(fileName, cancellationToken);

        return chunksRemoved;
    }
}

public record UploadResult(int OldChunksRemoved, bool Indexed);