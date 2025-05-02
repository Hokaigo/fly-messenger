using Messenger.Domain.Entities;
using Messenger.Domain.Repositories;
using Messenger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Messenger.Infrastructure.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly AppDbContext _context;

        public MessageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Message message)
        {
            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();
        }

        public async Task<Message?> GetByIdAsync(Guid id)
        {
            return await _context.Messages.FindAsync(id);
        }

        public async Task<IEnumerable<Message>> GetMessagesByChatIdAsync(Guid chatId)
        {
            return await _context.Messages.Where(m => m.ChatId == chatId).Include(m => m.User).OrderBy(m => m.DateSent).ToListAsync();
        }

        public async Task UpdateAsync(Message message)
        {
            _context.Messages.Update(message);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var msg = await _context.Messages.FindAsync(id);
            if (msg != null)
            {
                _context.Messages.Remove(msg);
                await _context.SaveChangesAsync();
            }
        }
    }
}
