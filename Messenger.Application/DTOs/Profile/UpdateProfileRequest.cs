using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Messenger.Application.DTOs.Profile
{
    public class UpdateProfileRequest
    {
        [MaxLength(50)] 
        public string? FirstName { get; set; } = "";
        [MaxLength(50)] 
        public string? LastName { get; set; } = "";
        [MaxLength(500)] 
        public string? Bio { get; set; } = "";
        public IFormFile? Avatar { get; set; }
    }
}
