using System.ComponentModel.DataAnnotations;

namespace Messenger.Web.ViewModels
{
    public class ChatsViewModel
    {
        [Required(ErrorMessage = "Please, enter user's GUID")]
        [Display(Name = "User's GUID")]
        public string SearchTargetUserId { get; set; }

        public List<ChatListItemViewModel> Chats { get; set; } = new();
    }
}