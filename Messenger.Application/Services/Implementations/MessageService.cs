using AutoMapper;
using Messenger.Application.DTOs.Chats;
using Messenger.Application.Services.Interfaces;
using Messenger.Domain.Entities;
using Messenger.Domain.Enums;
using Messenger.Domain.Repositories;
using Microsoft.AspNetCore.Http;

namespace Messenger.Application.Services.Implementations
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _msgRepo;
        private readonly IUserRepository _userRepo;
        private readonly IFileStorageService _storage;
        private readonly IMapper _mapper;

        public MessageService(IMessageRepository msgRepo, IUserRepository userRepo, IFileStorageService storage, IMapper mapper)
        {
            _msgRepo = msgRepo;
            _userRepo = userRepo;
            _storage = storage;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesAsync(Guid chatId)
        {
            var msgs = await _msgRepo.GetMessagesByChatIdAsync(chatId);
            return _mapper.Map<IEnumerable<MessageDto>>(msgs);
        }

        public async Task<MessageDto> SendMessageAsync(Guid chatId, Guid userId, string? text, IFormFile? file)
        {
            var dto = new SendMessageDto { ChatId = chatId, Text = text, File = file };
            var msg = _mapper.Map<Message>(dto);
            msg.UserId = userId;

            if (file != null && file.Length > 0)
            {
                var saved = await _storage.SaveAsync(file.OpenReadStream(), file.FileName, "uploads");
                msg.Type = MessageType.File;
                msg.FileUrl = saved;
                msg.FileName = file.FileName;
                msg.FileType = file.ContentType;
                msg.FileSize = file.Length;
            }

            await _msgRepo.AddAsync(msg);

            var result = _mapper.Map<MessageDto>(msg);
            var user = await _userRepo.GetByIdAsync(userId);
            result.UserName = user?.UserName ?? "Unknown";
            return result;
        }

        public async Task<MessageDto> EditMessageAsync(Guid messageId, string newText)
        {
            var msg = await _msgRepo.GetByIdAsync(messageId) ?? throw new KeyNotFoundException("Message not found");

            msg.Text = newText.Trim();
            await _msgRepo.UpdateAsync(msg);

            var dto = _mapper.Map<MessageDto>(msg);
            var user = await _userRepo.GetByIdAsync(msg.UserId);
            dto.UserName = user?.UserName ?? "Unknown";
            return dto;
        }

        public async Task<Guid> DeleteMessageAsync(Guid messageId)
        {
            var msg = await _msgRepo.GetByIdAsync(messageId) ?? throw new KeyNotFoundException("Message not found");

            var chatId = msg.ChatId;
            await _msgRepo.DeleteAsync(messageId);
            return chatId;
        }
    }
}
