using AutoMapper;
using Messenger.Application.DTOs.Profile;
using Messenger.Application.Services.Interfaces;
using Messenger.Domain.Entities;
using Messenger.Domain.Repositories;
using Microsoft.AspNetCore.Http;

namespace Messenger.Application.Services.Implementations
{
    public class ProfileService : IProfileService
    {
        private readonly IUserProfileRepository _profileRepo;
        private readonly IUserRepository _userRepo;
        private readonly IFileStorageService _storage;
        private readonly IMapper _mapper;

        public ProfileService(IUserProfileRepository profileRepo, IUserRepository userRepo, IFileStorageService storage, IMapper mapper)
        {
            _profileRepo = profileRepo;
            _userRepo = userRepo;
            _storage = storage;
            _mapper = mapper;
        }

        public async Task<UserProfileDto?> GetByUserIdAsync(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return null;

            var profile = user.Profile ?? new UserProfile { UserId = userId };
            var dto = _mapper.Map<UserProfileDto>(profile);
            dto.UserName = user.UserName;
            dto.IsOnline = false; 
            return dto;
        }

        public async Task UpdateAsync(Guid userId, UpdateProfileRequest request, IFormFile? avatarFile)
        {
            var existing = await _profileRepo.GetByUserIdAsync(userId);
            var profile = existing ?? new UserProfile { UserId = userId };

            _mapper.Map(request, profile);

            if (avatarFile != null && avatarFile.Length > 0)
            {
                var saved = await _storage.SaveAsync(avatarFile.OpenReadStream(), $"{userId}{Path.GetExtension(avatarFile.FileName)}", "uploads/avatars");
                profile.ProfilePicUrl = saved;
            }

            if (existing == null)
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
