using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Personalhandboksassistent.Models;

namespace Personalhandboksassistent.Services;

public class IndexerService
{
    private readonly SearchClient _search;
    private readonly SearchIndexerClient _indexer;
    private readonly string _indexerName;

    public IndexerService()
    {
        var endpoint = new Uri(Environment.GetEnvironmentVariable("AZURE_SEARCH_ENDPOINT")
            ?? throw new InvalidOperationException("AZURE_SEARCH_ENDPOINT saknas i .env"));
        var searchKey = Environment.GetEnvironmentVariable("AZURE_SEARCH_KEY")
            ?? throw new InvalidOperationException("AZURE_SEARCH_KEY saknas i .env");
        var index = Environment.GetEnvironmentVariable("AZURE_SEARCH_INDEX")
            ?? throw new InvalidOperationException("AZURE_SEARCH_INDEX saknas i .env");

        _indexerName = Environment.GetEnvironmentVariable("AZURE_SEARCH_INDEXER")
            ?? throw new InvalidOperationException("AZURE_SEARCH_INDEXER saknas i .env");

        var key = new AzureKeyCredential(searchKey);

        _search = new SearchClient(endpoint, index, key);
        _indexer = new SearchIndexerClient(endpoint, key);
    }

    public async Task<string> RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _indexer.RunIndexerAsync(_indexerName, cancellationToken);

            return "Startade indexering";
        }
        catch (RequestFailedException ex) when (ex.Status == 409)
        {
            return "Indexering körs redan";
        }
    }

    public async Task<bool> WaitUntilIndexedAsync(string fileName, int timeoutSeconds, CancellationToken cancellationToken = default)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(timeoutSeconds);
        var nextTrigger = DateTimeOffset.UtcNow;

        while (DateTimeOffset.UtcNow < deadline)
        {
            var chunks = await GetChunksAsync(cancellationToken);

            if (chunks.Any(chunk => string.Equals(chunk.Title, fileName, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            if (DateTimeOffset.UtcNow >= nextTrigger)
            {
                await RunAsync(cancellationToken);

                nextTrigger = DateTimeOffset.UtcNow.AddSeconds(15);
            }

            await Task.Delay(2000, cancellationToken);
        }

        return false;
    }

    private async Task<List<DocumentChunk>> GetChunksAsync(CancellationToken cancellationToken)
    {
        const int pageSize = 1000;

        var chunks = new List<DocumentChunk>();
        var skip = 0;

        while (true)
        {
            var options = new SearchOptions { Size = pageSize, Skip = skip };
            options.Select.Add("chunk_id");
            options.Select.Add("title");

            var response = await _search.SearchAsync<SearchDocument>("*", options, cancellationToken);
            var found = 0;

            await foreach (var hit in response.Value.GetResultsAsync())
            {
                found++;

                chunks.Add(new DocumentChunk
                {
                    Id = hit.Document.TryGetValue("chunk_id", out var id) ? id?.ToString() ?? string.Empty : string.Empty,
                    Title = hit.Document.TryGetValue("title", out var title) ? title?.ToString() : null
                });
            }

            if (found < pageSize)
            {
                return chunks;
            }

            skip += pageSize;
        }
    }

    public async Task<int> DeleteChunksAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var chunks = await GetChunksAsync(cancellationToken);

        var keys = chunks
            .Where(chunk => string.Equals(chunk.Title, fileName, StringComparison.OrdinalIgnoreCase))
            .Select(chunk => chunk.Id)
            .ToList();

        return await DeleteChunksAsync(keys, cancellationToken);
    }

    private async Task<int> DeleteChunksAsync(List<string> keys, CancellationToken cancellationToken)
    {
        if (keys.Count == 0)
        {
            return 0;
        }

        await _search.DeleteDocumentsAsync("chunk_id", keys, cancellationToken: cancellationToken);

        return keys.Count;
    }
}
