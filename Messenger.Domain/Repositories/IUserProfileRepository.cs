using Messenger.Domain.Entities;

namespace Messenger.Domain.Repositories
{
    public interface IUserProfileRepository
    {
        Task<UserProfile?> GetByUserIdAsync(Guid userId);
        Task AddAsync(UserProfile profile);
        Task UpdateAsync(UserProfile profile);
        Task DeleteByUserIdAsync(Guid userId);
    }
}
