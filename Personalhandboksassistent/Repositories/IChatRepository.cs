using Personalhandboksassistent.Models;

namespace Personalhandboksassistent.Repositories
{
    public interface IChatRepository
    {
        Task<Chat> GetAsync(string user, CancellationToken cancellationToken = default);

        Task SaveAsync(string user, Chat chat, CancellationToken cancellationToken = default);

        Task ClearAsync(string user, CancellationToken cancellationToken = default);
    }
}
