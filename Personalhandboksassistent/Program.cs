using Personalhandboksassistent.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IFileRepository, AzBlobFileRepository>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/testFile", (IFileRepository fileRepo) => fileRepo.ListAsync(""));

app.Run();
