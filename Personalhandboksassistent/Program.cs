using Personalhandboksassistent.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IFileRepository, AzBlobFileRepository>();

var app = builder.Build();

// Force construction of the file repo to bubble up configuration errors at startup instead of
// letting it fail on first request. Should this be handled better?.. Probably.
_ = app.Services.GetRequiredService<IFileRepository>();


app.MapGet("/", () => "Hello World!");

app.MapGet("/testFile", (IFileRepository fileRepo) => fileRepo.ListAsync(""));

app.Run();
