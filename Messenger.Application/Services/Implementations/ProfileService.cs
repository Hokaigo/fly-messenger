using Messenger.Application.DTOs.Profile;
using Messenger.Application.Services.Interfaces;
using Messenger.Domain.Entities;
using Messenger.Domain.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Messenger.Application.Services.Implementations
{
    public class ProfileService : IProfileService
    {
        private readonly IUserProfileRepository _profileRepo;
        private readonly IUserRepository _userRepo;
        private readonly IWebHostEnvironment _env;

        public ProfileService(IUserProfileRepository profileRepo, IUserRepository userRepo, IWebHostEnvironment env)
        {
            _profileRepo = profileRepo;
            _userRepo = userRepo;
            _env = env;
        }

        public async Task<UserProfileDto?> GetByUserIdAsync(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return null;

            var profile = user.Profile ?? new UserProfile { UserId = userId };

            return new UserProfileDto
            {
                UserId = user.Id,
                UserName = user.UserName,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                Bio = profile.Bio,
                ProfilePicUrl = profile.ProfilePicUrl
            };
        }

        public async Task UpdateAsync(Guid userId, UpdateProfileRequest request, IFormFile? avatarFile)
        {
            var existing = await _profileRepo.GetByUserIdAsync(userId);
            bool isNew = existing == null;
            var profile = isNew
                ? new UserProfile { UserId = userId }
                : existing!;

            if (!string.IsNullOrWhiteSpace(request.FirstName))
                profile.FirstName = request.FirstName!;
            if (!string.IsNullOrWhiteSpace(request.LastName))
                profile.LastName = request.LastName!;
            if (!string.IsNullOrWhiteSpace(request.Bio))
                profile.Bio = request.Bio!;

            if (avatarFile != null && avatarFile.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads", "avatars");
                Directory.CreateDirectory(uploads);

                var ext = Path.GetExtension(avatarFile.FileName);
                var fileName = $"{userId}{ext}";
                var filePath = Path.Combine(uploads, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await avatarFile.CopyToAsync(stream);

                profile.ProfilePicUrl = $"/uploads/avatars/{fileName}";
            }

            if (isNew)
                await _profileRepo.AddAsync(profile);
            else
                await _profileRepo.UpdateAsync(profile);
        }

        public async Task DeleteAccountAsync(Guid userId)
        {
            await _profileRepo.DeleteByUserIdAsync(userId);
            await _userRepo.DeleteAsync(userId); 
        }
    }
}
