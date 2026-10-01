using System.Text.Json;
using Personalhandboksassistent.Models;

namespace Personalhandboksassistent.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly string _chatPath;

        public ChatRepository(IWebHostEnvironment environment)
        {
            _chatPath = Path.Combine(environment.ContentRootPath, "Data", "Chats");
        }

        public async Task<Chat> GetAsync(string user, CancellationToken cancellationToken = default)
        {
            var path = PathFor(user);
            var file = new FileInfo(path);

            if (!file.Exists || file.Length == 0)
            {
                return new Chat();
            }

            await using var stream = System.IO.File.OpenRead(path);

            return await JsonSerializer.DeserializeAsync<Chat>(stream, cancellationToken: cancellationToken) ?? new Chat();
        }

        public async Task SaveAsync(string user, Chat chat, CancellationToken cancellationToken = default)
        {
            Directory.CreateDirectory(_chatPath);

            await using var stream = System.IO.File.Create(PathFor(user));

            await JsonSerializer.SerializeAsync(stream, chat, new JsonSerializerOptions { WriteIndented = true }, cancellationToken);
        }

        public Task ClearAsync(string user, CancellationToken cancellationToken = default)
        {
            var path = PathFor(user);

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            return Task.CompletedTask;
        }

        private string PathFor(string user) => Path.Combine(_chatPath, $"{user}-Chat.json");
    }
}