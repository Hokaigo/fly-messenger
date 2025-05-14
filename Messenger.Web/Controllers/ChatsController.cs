using Messenger.Application.DTOs.Chats;
using Messenger.Application.DTOs.Profile;
using Messenger.Application.MessageProcessing.interfaces;
using Messenger.Application.MessageProcessing.validation;
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

        private readonly ITextMessageHandler _textHandler;
        private readonly IFileMessageHandler _fileHandler;

        public ChatsController(IChatService chatService, IMessageService msgService, IHubContext<ChatHub> chatHub, IHubContext<ChatListHub> chatListHub,
            IUserService userService, IProfileService profileService, IOnlineUserTracker tracker, ITextMessageHandler textHandler,
            IFileMessageHandler fileHandler)
        {
            _chatService = chatService;
            _msgService = msgService;
            _chatHub = chatHub;
            _chatListHub = chatListHub;
            _userService = userService;
            _profileService = profileService;
            _tracker = tracker;
            _textHandler = textHandler;
            _fileHandler = fileHandler;
        }

        public async Task<IActionResult> Index()
        {
            var me = GetUserId();
            var summaries = await _chatService.GetChatSummariesAsync(me);
            var nonEmpty = summaries
             .Select(s => new ChatListItemViewModel
             {
                 ChatId = s.ChatId,
                 OtherUserName = s.OtherUserName,
                 LastMessage = s.LastMessage,
                 LastMessageTime = s.LastMessageTime
             })
             .ToList();


            return View(new ChatsViewModel { Chats = nonEmpty });
        }

        public async Task<IActionResult> Open(Guid chatId)
        {
            var me = GetUserId();
            var chat = await _chatService.GetChatByIdAsync(chatId);

            if (chat == null) return NotFound();
            if (!chat.Members.Any(m => m.Id == me)) return Forbid();

            var otherUser = chat.Members.FirstOrDefault(m => m.Id != me);
            if (otherUser == null)
            {
                TempData["ErrorMessage"] = "The other participant in this chat has been removed.";
                return RedirectToAction("Index");
            }

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
                return BadRequest(new { error = "An error occurred while sending a message." });

            try
            {
                var dto = await _msgService.SendMessageAsync(chatId, GetUserId(), text, file);
                await BroadcastMessage(dto, chatId);
                return Ok();
            }
            catch (InvalidMessageException mex)
            {
                return BadRequest(new { error = mex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Server error occurred" });
            }
        }



        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMessage(Guid messageId, string newText)
        {
            try
            {
                var dto = await _msgService.EditMessageAsync(messageId, newText);
                await _chatHub.Clients.Group(dto.ChatId.ToString()).SendAsync("MessageEdited", dto);
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
                await _chatHub.Clients.Group(chatId.ToString()).SendAsync("MessageDeleted", messageId);
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
                return await IndexWithModel(me, model);

            var chatDto = await _chatService.SearchOrCreatePrivateChatAsync(me, targetId);
            return RedirectToAction("Open", new { chatId = chatDto.Id });
        }

        private async Task<IActionResult> IndexWithModel(Guid me, ChatsViewModel model)
        {
            var summaries = await _chatService.GetChatSummariesAsync(me);
            model.Chats = summaries
                  .Select(s => new ChatListItemViewModel
                  {
                      ChatId = s.ChatId,
                      OtherUserName = s.OtherUserName,
                      LastMessage = s.LastMessage,
                      LastMessageTime = s.LastMessageTime
                  })
                  .ToList();
            return View("Index", model);
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private async Task BroadcastMessage(MessageDto dto, Guid chatId)
        {
            var me = GetUserId();

            await _chatHub.Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", dto);

            var summaryForMe = await _chatService.GetChatSummaryAsync(chatId, me);
            await _chatListHub.Clients.Group(me.ToString()).SendAsync("ChatListUpdated", summaryForMe);

            var otherUser = (await _chatService.GetChatByIdAsync(chatId)).Members.FirstOrDefault(m => m.Id != me);
            if (otherUser != null)
            {
                var summaryForOther = await _chatService.GetChatSummaryAsync(chatId, otherUser.Id);
                await _chatListHub.Clients.Group(otherUser.Id.ToString()).SendAsync("ChatListUpdated", summaryForOther);
            }
        }

    }
}
