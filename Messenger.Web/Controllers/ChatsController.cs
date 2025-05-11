using Messenger.Application.DTOs.Chats;
using Messenger.Application.DTOs.Profile;
using Messenger.Application.Services.Interfaces;
using Messenger.CrossCutting.Services;
using Messenger.Web.Hubs;
using Messenger.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Messenger.Web.Controllers
{
    [Authorize]
    public class ChatsController : Controller
    {
        private readonly IChatService _chatService;
        private readonly IMessageService _msgService;
        private readonly IUserService _userService;
        private readonly IProfileService _profileService;
        private readonly IHubContext<ChatHub> _chatHub;
        private readonly IHubContext<ChatListHub> _chatListHub;
        private readonly IOnlineUserTracker _tracker;

        private const long FILE_SIZE_LIMIT = 15 * 1024 * 1024;
        private static readonly string[] ALLOWED_EXT = { ".png", ".jpg", ".jpeg", ".gif", ".pdf", ".docx", ".mp4", ".mp3" };

        public ChatsController(IChatService chatService, IMessageService msgService, IHubContext<ChatHub> chatHub, IHubContext<ChatListHub> chatListHub,
            IUserService userService, IProfileService profileService, IOnlineUserTracker tracker)
        {
            _chatService = chatService;
            _msgService = msgService;
            _chatHub = chatHub;
            _chatListHub = chatListHub;
            _userService = userService;
            _profileService = profileService;
            _tracker = tracker;
        }

        public async Task<IActionResult> Index()
        {
            var me = GetUserId();
            var summaries = await _chatService.GetChatSummariesAsync(me);
            var nonEmpty = summaries.Where(s => s.LastMessageTime.HasValue).ToList();

            var vm = new ChatsViewModel
            {
                Chats = nonEmpty.Select(s => new ChatListItemViewModel
                {
                    ChatId = s.ChatId,
                    OtherUserName = s.OtherUserName,
                    LastMessage = s.LastMessage,
                    LastMessageTime = s.LastMessageTime
                }).ToList()
            };
            return View(vm);
        }

        public async Task<IActionResult> Open(Guid chatId)
        {
            var me = GetUserId();
            var chat = await _chatService.GetChatByIdAsync(chatId);
            if (chat == null) return NotFound();
            if (!chat.Members.Any(m => m.Id == me)) return Forbid();

            var otherUser = chat.Members.First(m => m.Id != me);
            var otherProfile = await _profileService.GetByUserIdAsync(otherUser.Id);

            ViewBag.OtherUserName = otherUser.UserName;
            ViewBag.OtherUserAvatar = otherProfile?.ProfilePicUrl;
            ViewBag.UserInitial = !string.IsNullOrEmpty(otherUser.UserName) ? otherUser.UserName[0].ToString() : "?";
            ViewBag.UploadError = TempData["UploadError"];

            var participants = new List<UserProfileDto>();
            foreach (var member in chat.Members)
            {
                var profile = await _profileService.GetByUserIdAsync(member.Id);
                participants.Add(new UserProfileDto
                {
                    UserId = member.Id,
                    UserName = member.UserName,
                    ProfilePicUrl = profile?.ProfilePicUrl,
                    IsOnline = _tracker.IsUserOnline(member.Id)
                });
            }
            ViewBag.ChatParticipants = participants;

            return View(chat);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(Guid chatId, string? text, IFormFile? file)
        {
            if (chatId == Guid.Empty)
                return BadRequest("An error occurred while sending the message.");

            if (!ValidateFile(file, out var fileError))
            {
                TempData["UploadError"] = fileError;
                return RedirectToAction("Open", new { chatId });
            }

            var dto = await _msgService.SendMessageAsync(chatId, GetUserId(), text, file);
            await BroadcastMessage(dto, chatId);
            return Ok();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMessage(Guid messageId, string newText)
        {
            try
            {
                var dto = await _msgService.EditMessageAsync(messageId, newText);
                await _chatHub.Clients.Group(dto.ChatId.ToString())
                                      .SendAsync("MessageEdited", dto);
                return Ok(dto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage(Guid messageId)
        {
            try
            {
                var chatId = await _msgService.DeleteMessageAsync(messageId);
                await _chatHub.Clients.Group(chatId.ToString())
                                      .SendAsync("MessageDeleted", messageId);
                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(ChatsViewModel model)
        {
            var me = GetUserId();
            if (!Guid.TryParse(model.SearchTargetUserId, out var targetId))
            {
                ModelState.AddModelError(nameof(model.SearchTargetUserId), "Wrong GUID format.");
            }
            else if (targetId == me)
            {
                ModelState.AddModelError(nameof(model.SearchTargetUserId), "You can't start a chat with yourself");
            }
            else
            {
                var userDto = await _userService.GetByIdAsync(targetId);
                if (userDto == null)
                    ModelState.AddModelError(nameof(model.SearchTargetUserId), "User not found");
            }

            if (!ModelState.IsValid)
            {
                var summaries = await _chatService.GetChatSummariesAsync(me);
                var nonEmpty = summaries.Where(s => s.LastMessageTime.HasValue).ToList();

                model.Chats = nonEmpty.Select(s => new ChatListItemViewModel
                {
                    ChatId = s.ChatId,
                    OtherUserName = s.OtherUserName,
                    LastMessage = s.LastMessage,
                    LastMessageTime = s.LastMessageTime
                }).ToList();

                return View("Index", model);
            }

            var chatDto = await _chatService.SearchOrCreatePrivateChatAsync(me, targetId);
            return RedirectToAction("Open", new { chatId = chatDto.Id });
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private bool ValidateFile(IFormFile? file, out string error)
        {
            error = string.Empty;
            if (file == null) return true;

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedList = string.Join(", ", ALLOWED_EXT);

            if (!ALLOWED_EXT.Contains(ext))
            {
                error = $"File extension \"{ext}\" is not supported (allowed: {allowedList})";
                return false;
            }

            if (file.Length > FILE_SIZE_LIMIT)
            {
                error = $"File too large (maximum {FILE_SIZE_LIMIT / (1024 * 1024)} MB).";
                return false;
            }

            return true;
        }

        private async Task BroadcastMessage(MessageDto dto, Guid chatId)
        {
            var me = GetUserId();

            await _chatHub.Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", dto);

            var summary = await _chatService.GetChatSummaryAsync(chatId, me);

            await _chatListHub.Clients.Group(me.ToString()).SendAsync("ChatListUpdated", summary);

            var otherUser = (await _chatService.GetChatByIdAsync(chatId)).Members.FirstOrDefault(m => m.Id != me);

            if (otherUser != null)
            {
                await _chatListHub.Clients.Group(otherUser.Id.ToString()).SendAsync("ChatListUpdated", summary);
            }
        }
    }
}
