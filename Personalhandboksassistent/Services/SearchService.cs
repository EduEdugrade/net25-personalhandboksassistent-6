using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Personalhandboksassistent.Models;

namespace Personalhandboksassistent.Services;

public class SearchService
{
    private readonly SearchClient _client;
    private readonly string _semanticConfig;
    private readonly int _topK;

    public SearchService()
    {
        var endpoint = Environment.GetEnvironmentVariable("AZURE_SEARCH_ENDPOINT")
            ?? throw new InvalidOperationException("AZURE_SEARCH_ENDPOINT saknas i .env");
        var key = Environment.GetEnvironmentVariable("AZURE_SEARCH_KEY")
            ?? throw new InvalidOperationException("AZURE_SEARCH_KEY saknas i .env");
        var index = Environment.GetEnvironmentVariable("AZURE_SEARCH_INDEX")
            ?? throw new InvalidOperationException("AZURE_SEARCH_INDEX saknas i .env");

        _semanticConfig = Environment.GetEnvironmentVariable("AZURE_SEARCH_SEMANTIC_CONFIG")
            ?? throw new InvalidOperationException("AZURE_SEARCH_SEMANTIC_CONFIG saknas i .env");
        _topK = int.TryParse(Environment.GetEnvironmentVariable("SEARCH_TOP_K"), out var topK) ? topK : 5;

        _client = new SearchClient(new Uri(endpoint), index, new AzureKeyCredential(key));
    }

    public async Task<List<DocumentChunk>> SearchAsync(string question, CancellationToken cancellationToken = default)
    {
        var options = new SearchOptions
        {
            Size = _topK,
            QueryType = SearchQueryType.Semantic,
            SemanticSearch = new SemanticSearchOptions
            {
                SemanticConfigurationName = _semanticConfig
            },
            VectorSearch = new VectorSearchOptions
            {
                Queries =
                {
                    new VectorizableTextQuery(question)
                    {
                        KNearestNeighborsCount = _topK * 2,
                        Fields = { "text_vector" }
                    }
                }
            }
        };

        options.Select.Add("chunk_id");
        options.Select.Add("parent_id");
        options.Select.Add("title");
        options.Select.Add("chunk");

        var response = await _client.SearchAsync<SearchDocument>(question, options, cancellationToken);

        var chunks = new List<DocumentChunk>();
        var position = 0;

        await foreach (var hit in response.Value.GetResultsAsync())
        {
            chunks.Add(new DocumentChunk
            {
                Id = Field(hit.Document, "chunk_id"),
                DocumentId = Field(hit.Document, "parent_id"),
                Title = Field(hit.Document, "title"),
                Content = Field(hit.Document, "chunk"),
                ChunkIndex = position++,
                Score = hit.Score
            });
        }

        return chunks;
    }

    private static string Field(SearchDocument document, string name)
        => document.TryGetValue(name, out var value) ? value?.ToString() ?? string.Empty : string.Empty;
}
