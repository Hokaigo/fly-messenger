using Messenger.Application.DTOs.Chats;
using Messenger.Application.Services.Interfaces;
using Messenger.Domain.Entities;
using Messenger.Domain.Enums;
using Messenger.Domain.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Messenger.Application.Services.Implementations
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _msgRepo;
        private readonly IUserRepository _userRepo;
        private readonly IWebHostEnvironment _env;

        public MessageService(IMessageRepository msgRepo, IUserRepository userRepo, IWebHostEnvironment env)
        {
            _msgRepo = msgRepo;
            _userRepo = userRepo;
            _env = env;
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesAsync(Guid chatId)
        {
            var msgs = await _msgRepo.GetMessagesByChatIdAsync(chatId);
            return msgs.Select(m => ToMessageDto(m));
        }

        public async Task<MessageDto> SendMessageAsync(Guid chatId, Guid userId, string? text, IFormFile? file)
        {
            var msg = new Message
            {
                ChatId = chatId,
                UserId = userId,
                DateSent = DateTime.UtcNow,
                Type = MessageType.Text,
                Text = (text ?? string.Empty).Trim()
            };

            if (file is { Length: > 0 })
            {
                var uploadFolder = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadFolder);

                var uniqueName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var physicalPath = Path.Combine(uploadFolder, uniqueName);
                await using var stream = File.Create(physicalPath);
                await file.CopyToAsync(stream);

                msg.Type = MessageType.File;
                msg.FileUrl = $"/uploads/{uniqueName}";
                msg.FileName = file.FileName;
                msg.FileType = file.ContentType;
                msg.FileSize = file.Length;
            }

            await _msgRepo.AddAsync(msg);
            return await BuildDtoAsync(msg);
        }


        public async Task<MessageDto> EditMessageAsync(Guid messageId, string newText)
        {
            var msg = await _msgRepo.GetByIdAsync(messageId)
                      ?? throw new KeyNotFoundException("Message is not found");

            msg.Text = newText.Trim();
            await _msgRepo.UpdateAsync(msg);
            return await BuildDtoAsync(msg);
        }

        public async Task<Guid> DeleteMessageAsync(Guid messageId)
        {
            var msg = await _msgRepo.GetByIdAsync(messageId) ?? throw new KeyNotFoundException("Message is not found");
            var chatId = msg.ChatId;

            await _msgRepo.DeleteAsync(messageId);
            return chatId;
        }


        private static MessageDto ToMessageDto(Message m) => new()
        {
            Id = m.Id,
            ChatId = m.ChatId,
            UserId = m.UserId,
            UserName = m.User.UserName,
            DateSent = m.DateSent,
            Type = m.Type,
            Text = m.Text,
            FileUrl = m.FileUrl,
            FileName = m.FileName,
            FileType = m.FileType,
            FileSize = m.FileSize
        };

        private async Task<MessageDto> BuildDtoAsync(Message m)
        {
            var user = await _userRepo.GetByIdAsync(m.UserId);
            var dto = ToMessageDto(m);
            dto.UserName = user?.UserName ?? "Unknown User";
            return dto;
        }
    }
}
