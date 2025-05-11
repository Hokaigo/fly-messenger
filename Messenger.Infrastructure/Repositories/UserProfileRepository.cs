using Messenger.Domain.Entities;
using Messenger.Domain.Repositories;
using Messenger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Messenger.Infrastructure.Repositories
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly AppDbContext _context;

        public UserProfileRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(UserProfile profile)
        {
            await _context.UserProfiles.AddAsync(profile);
            await _context.SaveChangesAsync();
        }

        public async Task<UserProfile?> GetByUserIdAsync(Guid userId)
        {
            return await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == userId);
        }

        public async Task UpdateAsync(UserProfile profile)
        {
            _context.UserProfiles.Update(profile);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByUserIdAsync(Guid userId)
        {
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == userId);
            if (profile != null)
            {
                _context.UserProfiles.Remove(profile);
                await _context.SaveChangesAsync();
            }
        }

    }
}
