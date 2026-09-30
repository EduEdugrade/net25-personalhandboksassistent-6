using DotNetEnv;
using Personalhandboksassistent.Services;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<SearchService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/search", async (string question, SearchService search) =>
{
    var chunks = await search.SearchAsync(question);

    return Results.Ok(chunks.Select(chunk => new
    {
        chunk.Id,
        chunk.Title,
        chunk.DocumentId,
        chunk.Score,
        Text = chunk.Content.Length > 300 ? chunk.Content[..300] + "..." : chunk.Content
    }));
});

app.Run();
