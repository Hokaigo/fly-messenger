namespace Messenger.Application.DTOs.Profile
{
    public class UserProfileDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Bio { get; set; } = "";
        public string? ProfilePicUrl { get; set; }
        public bool IsOnline { get; set; }
    }
}
