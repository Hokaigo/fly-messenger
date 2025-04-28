namespace Messenger.Domain.Entities
{
    public class UserProfile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Bio {  get; set; } = string.Empty;
        public string ProfilePicUrl { get; set; } = string.Empty;

        public User User { get; set; } = null!;
    }
}
