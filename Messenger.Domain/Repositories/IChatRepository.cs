using Messenger.Domain.Entities;

namespace Messenger.Domain.Repositories
{
    public interface IChatRepository
    {
        Task<Chat?> GetByIdAsync(Guid id);
        Task<IEnumerable<Chat>> GetUserChatsAsync(Guid userId);
        Task AddAsync(Chat chat);   
        Task UpdateAsync(Chat chat);
        Task DeleteAsync(Guid id);
        Task<Chat?> GetPrivateChatBetweenUsersAsync(Guid user1Id, Guid user2Id);

    }
}
