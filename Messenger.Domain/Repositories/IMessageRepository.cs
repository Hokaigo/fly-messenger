using Messenger.Domain.Entities;

namespace Messenger.Domain.Repositories
{
    public interface IMessageRepository
    {
        Task<Message?> GetByIdAsync(Guid id);
        Task<IEnumerable<Message>> GetMessagesByChatIdAsync(Guid chatId);
        Task AddAsync(Message message);
        Task UpdateAsync(Message message);
        Task DeleteAsync(Guid id);
    }
}
