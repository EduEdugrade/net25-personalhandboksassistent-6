using Azure;
using DotNetEnv;
using Personalhandboksassistent.Repositories;
using Personalhandboksassistent.Services;
using Personalhandboksassistent.Models;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<SearchService>();
builder.Services.AddSingleton<AnswerService>();
builder.Services.AddSingleton<ChatService>();
builder.Services.AddSingleton<IndexerService>();
builder.Services.AddSingleton<DocumentService>();
builder.Services.AddSingleton<IFileRepository, AzBlobFileRepository>();
builder.Services.AddSingleton<IChatRepository, ChatRepository>();

var app = builder.Build();

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

app.MapGet("/api/documents", async (DocumentService documents) =>
    Results.Ok(await documents.ListAsync()));

app.MapPost("/api/addFile", async (IFormFile file, DocumentService documents) =>
{
    var oldChunks = await documents.UploadAsync(file.FileName, file.OpenReadStream(), file.ContentType);

    return Results.Ok(new { name = file.FileName, oldChunksRemoved = oldChunks });
}).DisableAntiforgery();

app.MapDelete("/api/documents/{fileName}", async (string fileName, DocumentService documents) =>
{
    var chunksRemoved = await documents.DeleteAsync(fileName);

    return Results.Ok(new { name = fileName, chunksRemoved });
});

app.MapPost("/api/documents/cleanup", async (DocumentService documents) =>
    Results.Ok(new { removedChunks = await documents.CleanOrphansAsync() }));

app.MapPost("/api/answer", async (AnswerRequest request, ChatService chat) =>
{
    var reply = await chat.AskAsync("User", request.Message);

    return Results.Ok(new
    {
        answer = reply.Message,
        reply.IsFound,
        reply.Sources
    });
});

app.MapDelete("/api/history", async (ChatService chat) =>
{
    await chat.ClearAsync("User");

    return Results.Ok(new { message = "Chatthistoriken är rensad." });
});

app.Run();
