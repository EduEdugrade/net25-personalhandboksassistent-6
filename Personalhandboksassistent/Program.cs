using DotNetEnv;
using Personalhandboksassistent.Repositories;
using Personalhandboksassistent.Services;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<SearchService>();
builder.Services.AddSingleton<IFileRepository, AzBlobFileRepository>();

var app = builder.Build();

// Force construction of the file repo to bubble up configuration errors at startup instead of
// letting it fail on first request. Should this be handled better?.. Probably.
_ = app.Services.GetRequiredService<IFileRepository>();

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

app.MapGet("/testFile", (IFileRepository fileRepo) => fileRepo.ListAsync(""));

app.Run();
