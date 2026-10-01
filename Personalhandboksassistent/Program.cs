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

_ = app.Services.GetRequiredService<ChatService>();
_ = app.Services.GetRequiredService<DocumentService>();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    context.Response.ContentType = "application/json";

    await context.Response.WriteAsJsonAsync(new { message = "Något gick fel på servern." });
}));

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/documents", async (DocumentService documents, HttpContext context) =>
    Results.Ok(await documents.ListAsync(context.RequestAborted)));

app.MapGet("/api/documents/{fileName}", async (string fileName, IFileRepository files, HttpContext context) =>
{
    try
    {
        var file = await files.GetAsync(fileName, context.RequestAborted);

        return Results.File(file.Content, file.ContentType, enableRangeProcessing: true);
    }
    catch (RequestFailedException ex) when (ex.Status == 404)
    {
        return Results.NotFound(new { message = "Dokumentet finns inte." });
    }
});

app.MapPost("/api/addFile", async (IFormFile file, DocumentService documents, HttpContext context) =>
{
    if (!DocumentRules.IsAllowed(file.FileName))
    {
        return Results.BadRequest(new { message = $"Filtypen stöds inte. Tillåtna typer: {DocumentRules.AllowedList}." });
    }

    if (file.Length > DocumentRules.MaxBytes)
    {
        return Results.BadRequest(new { message = $"Filen är för stor. Max {DocumentRules.MaxMegabytes} MB." });
    }

    var result = await documents.UploadAsync(file.FileName, file.OpenReadStream(), file.ContentType, context.RequestAborted);

    return Results.Ok(new { name = file.FileName, oldChunksRemoved = result.OldChunksRemoved, indexed = result.Indexed });
}).DisableAntiforgery();

app.MapDelete("/api/documents/{fileName}", async (string fileName, DocumentService documents, HttpContext context) =>
{
    var chunksRemoved = await documents.DeleteAsync(fileName, context.RequestAborted);

    return Results.Ok(new { name = fileName, chunksRemoved });
});

app.MapPost("/api/index/cleanup", async (DocumentService documents, HttpContext context) =>
    Results.Ok(new { removedChunks = await documents.CleanOrphansAsync(context.RequestAborted) }));

app.MapPost("/api/answer", async (AnswerRequest request, ChatService chat, HttpContext context) =>
{
    var reply = await chat.AskAsync("User", request.Message, context.RequestAborted);

    return Results.Ok(new
    {
        answer = reply.Message,
        reply.IsFound,
        reply.Sources
    });
});

app.MapGet("/api/history", async (ChatService chat, HttpContext context) =>
    Results.Ok(await chat.GetHistoryAsync("User", context.RequestAborted)));

app.MapDelete("/api/history", async (ChatService chat, HttpContext context) =>
{
    await chat.ClearAsync("User", context.RequestAborted);

    return Results.Ok(new { message = "Chatthistoriken är rensad." });
});

app.Run();
