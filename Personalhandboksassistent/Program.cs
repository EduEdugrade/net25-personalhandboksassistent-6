using DotNetEnv;
using Personalhandboksassistent.Repositories;
using Personalhandboksassistent.Services;
using Personalhandboksassistent.Models;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<SearchService>();
builder.Services.AddSingleton<AnswerService>();
builder.Services.AddSingleton<ChatService>();
builder.Services.AddSingleton<IFileRepository, AzBlobFileRepository>();

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

app.MapPost("/api/chat", async (ChatRequest request, ChatService chat) =>
{
    var reply = await chat.AskAsync(request.Question);

    return Results.Ok(reply);
});

app.Run();
