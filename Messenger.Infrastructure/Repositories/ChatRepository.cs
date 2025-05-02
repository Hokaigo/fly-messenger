using Messenger.Domain.Entities;
using Messenger.Domain.Enums;
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
            return await _context.Chats.Include(c => c.Members).Include(c => c.Messages).ThenInclude(m => m.User)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Chat>> GetUserChatsAsync(Guid userId)
        {
            return await _context.Chats.Include(c => c.Members).Where(chat => chat.Members.Any(user => user.Id == userId)).ToListAsync();
        }

        public async Task UpdateAsync(Chat chat)
        {
            _context.Chats.Update(chat);
            await _context.SaveChangesAsync();
        }

        public async Task<Chat?> GetPrivateChatBetweenUsersAsync(Guid user1Id, Guid user2Id)
        {
            return await _context.Chats.Include(c => c.Members).FirstOrDefaultAsync(c => c.Type == ChatType.Private &&
                    c.Members.Any(m => m.Id == user1Id) && c.Members.Any(m => m.Id == user2Id));
        }
    }
}
