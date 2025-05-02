using Messenger.Application.DTOs.Chats;
using Messenger.Application.DTOs.Users;

namespace Messenger.Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<bool> UserExistsByEmailAsync(string email);
        Task<UserDto?> GetByIdAsync(Guid userId);
    }
}
