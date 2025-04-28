using Messenger.Domain.Entities;
using Messenger.Domain.Repositories;
using Messenger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Messenger.Infrastructure.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly AppDbContext _context;

        public ChatRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Chat chat)
        {
            await _context.Chats.AddAsync(chat);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var chat = await _context.Chats.FindAsync(id);
            if (chat != null)
            {
                _context.Chats.Remove(chat);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Chat?> GetByIdAsync(Guid id)
        {
            return await _context.Chats.FindAsync(id);
        }

        public async Task<IEnumerable<Chat>> GetUserChatsAsync(Guid userId)
        {
            return await _context.Chats.Where(chat => chat.Members.Any(user => user.Id == userId)).ToListAsync();
        }

        public async Task UpdateAsync(Chat chat)
        {
            _context.Chats.Update(chat);
            await _context.SaveChangesAsync();
        }
    }
}
