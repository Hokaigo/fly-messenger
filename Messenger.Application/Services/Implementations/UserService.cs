using Messenger.Application.DTOs.Chats;
using Messenger.Application.DTOs.Users;
using Messenger.Application.Services.Interfaces;
using Messenger.Domain.Entities;
using Messenger.Domain.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace Messenger.Application.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo) => _repo = repo;

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var user = await _repo.GetByEmailAsync(request.Email);
            bool success = user != null && VerifyPassword(request.Password, user.PasswordHash);
            if (!success)
                throw new Exception("Invalid email or password.");
            return new LoginResponse { UserId = user!.Id };
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest req)
        {
            if (req.Password != req.ConfirmPassword)
                throw new Exception("Passwords don't match.");

            var exists = await _repo.GetByEmailAsync(req.Email);
            if (exists != null)
                throw new Exception("User already exists.");

            var hash = HashPassword(req.Password);
            var u = new User
            {
                UserName = req.UserName,
                Email = req.Email,
                PasswordHash = hash
            };
            await _repo.AddAsync(u);
            return new RegisterResponse { UserId = u.Id };
        }

        private string HashPassword(string pwd)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(pwd)));
        }

        private bool VerifyPassword(string pwd, string stored) => HashPassword(pwd) == stored;

        public async Task<bool> UserExistsByEmailAsync(string email)
        {
            var user = await _repo.GetByEmailAsync(email);
            return user != null;
        }

        public async Task<UserDto?> GetByIdAsync(Guid userId)
        {
            var user = await _repo.GetByIdAsync(userId); 
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };
        }
    }
}
