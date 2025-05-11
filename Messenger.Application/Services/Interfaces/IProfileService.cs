using Messenger.Application.DTOs.Profile;
using Microsoft.AspNetCore.Http;

namespace Messenger.Application.Services.Interfaces
{
    public interface IProfileService
    {
        Task<UserProfileDto?> GetByUserIdAsync(Guid userId);
        Task UpdateAsync(Guid userId, UpdateProfileRequest request, IFormFile? avatarFile);
        Task DeleteAccountAsync(Guid userId);
    }
}
