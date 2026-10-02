
using Azure;

using DotNetEnv;
using Personalhandboksassistent.Repositories;
using Personalhandboksassistent.Services;
using Personalhandboksassistent.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var jwt = builder.Configuration.GetSection("Jwt");
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorization(options =>
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin")));

builder.Services.AddSingleton<SearchService>();
builder.Services.AddSingleton<AnswerService>();
builder.Services.AddSingleton<ChatService>();
builder.Services.AddSingleton<IndexerService>();
builder.Services.AddSingleton<DocumentService>();
builder.Services.AddSingleton<IFileRepository, AzBlobFileRepository>();
builder.Services.AddSingleton<IChatRepository, ChatRepository>();
builder.Services.AddSingleton<JwtTokenService>();

var app = builder.Build();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    context.Response.ContentType = "application/json";

    await context.Response.WriteAsJsonAsync(new { message = "Något gick fel på servern." });
}));

app.UseAuthentication();
app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapOpenApi();

var users = new List<AppUser>{
    new("1", "admin", BCrypt.Net.BCrypt.HashPassword("admin-pw"), "admin"),
    new("2", "user", BCrypt.Net.BCrypt.HashPassword("user-pw"), "user"),
};

app.MapPost("/login", (LoginRequest req, JwtTokenService tokenService) =>
{

    if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest(new { error = "användarnamn och lösenord krävs" });

    var user = users.FirstOrDefault(u => u.Username == req.Username);

    var ok = user is not null && BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
    if (!ok) return Results.Unauthorized();

    var token = tokenService.CreateToken(user!);
    return Results.Ok(new
    {
        token,
        user = new { id = user!.Id, username = user.Username, role = user.Role }
    });
});

var api = app.MapGroup("/api").RequireAuthorization();

var admin = api.MapGroup("").RequireAuthorization("AdminOnly");

admin.MapGet("/documents", async (DocumentService documents, HttpContext context) =>
    Results.Ok(await documents.ListAsync(context.RequestAborted)));

admin.MapPost("/addFile", async (IFormFile file, DocumentService documents, HttpContext context) =>
{
    if (!DocumentRules.IsAllowed(file.FileName))
    {
        return Results.BadRequest(new { message = $"Filtypen stöds inte. Tillåtna typer: {DocumentRules.AllowedList}." });
    }

    if (file.Length > DocumentRules.MaxBytes)
    {
        return Results.BadRequest(new { message = $"Filen är för stor. Max {DocumentRules.MaxMegabytes} MB." });
    }

    var result = await documents.UploadAsync(file.FileName, file.OpenReadStream(), DocumentRules.ContentTypeOf(file.FileName), context.RequestAborted);

    return Results.Ok(new
    {
        name = file.FileName,
        oldChunksRemoved = result.OldChunksRemoved,
        indexed = result.Indexed,
        message = result.Indexed
            ? "Dokumentet laddades upp och är nu sökbart."
            : "Dokumentet laddades upp, men indexeringen är inte klar ännu. Det kan dröja en stund innan det går att söka efter det."
    });
}).DisableAntiforgery();

admin.MapDelete("/documents/{fileName}", async (string fileName, DocumentService documents, HttpContext context) =>
{
    try
    {
        var chunksRemoved = await documents.DeleteAsync(fileName, context.RequestAborted);

        return Results.Ok(new { name = fileName, chunksRemoved });
    }
    catch (RequestFailedException ex) when (ex.Status == 404)
    {
        return Results.NotFound(new { message = "Dokumentet finns inte." });
    }
});

api.MapGet("/documents/{fileName}", async (string fileName, IFileRepository files, HttpContext context) =>
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

api.MapPost("/answer/stream", async (AnswerRequest request, ChatService chat, HttpContext context) =>
{
    var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

    if (string.IsNullOrEmpty(userId))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return;
    }

    context.Response.ContentType = "text/plain; charset=utf-8";
    context.Response.Headers["Cache-Control"] = "no-cache";

    await foreach (var delta in chat.AskStreamAsync(userId, request.Message, context.RequestAborted))
    {
        await context.Response.WriteAsync(delta, context.RequestAborted);
        await context.Response.Body.FlushAsync(context.RequestAborted);
    }
});

api.MapGet("/history", async (ChatService chat, HttpContext context) =>
{
    var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

    return Results.Ok(await chat.GetHistoryAsync(userId, context.RequestAborted));
});

api.MapDelete("/history", async (ChatService chat, HttpContext context) =>
{
    var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

    await chat.ClearAsync(userId, context.RequestAborted);

    return Results.Ok(new { message = "Chatthistoriken är rensad." });
});

app.Run();

public record LoginRequest(string Username, string Password);
