using AutoMapper;
using Messenger.Application.DTOs.Chats;
using Messenger.Application.MessageProcessing.interfaces;
using Messenger.Application.Services.Interfaces;
using Messenger.Domain.Entities;
using Messenger.Domain.Repositories;
using Microsoft.AspNetCore.Http;

namespace Messenger.Application.Services.Implementations
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _msgRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;
        private readonly IMessageHandler _textHandler;
        private readonly IMessageHandler _fileHandler;

        public MessageService(IMessageRepository msgRepo, IUserRepository userRepo, IMapper mapper, ITextMessageHandler textMessageHandler,
            IFileMessageHandler fileMessageHandler)
        {
            _msgRepo = msgRepo;
            _userRepo = userRepo;
            _mapper = mapper;
            _textHandler = textMessageHandler;
            _fileHandler = fileMessageHandler;

            _fileHandler.SetNext(_textHandler);
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesAsync(Guid chatId)
        {
            var entities = await _msgRepo.GetMessagesByChatIdAsync(chatId);
            var dtos = _mapper.Map<IEnumerable<MessageDto>>(entities);
            foreach (var d in dtos)
            {
                var u = await _userRepo.GetByIdAsync(d.UserId);
                d.UserName = u?.UserName ?? "Unknown"; 
            }
            return dtos;
        }

        public async Task<MessageDto> SendMessageAsync(Guid chatId, Guid userId, string? text, IFormFile? file)
        {
            var dto = await _fileHandler.ProcessAsync(chatId, userId, text, file);

            var user = await _userRepo.GetByIdAsync(userId);
            dto.UserName = user?.UserName ?? "Unknown"; 

            var entity = _mapper.Map<Message>(dto);
            await _msgRepo.AddAsync(entity);

            dto.UserName = user?.UserName ?? "Unknown";


            dto.Id = entity.Id;
            return dto;
        }

        public async Task<MessageDto> EditMessageAsync(Guid messageId, string newText)
        {
            var entity = await _msgRepo.GetByIdAsync(messageId) ?? throw new KeyNotFoundException("Message not found");

            entity.Text = newText.Trim();
            await _msgRepo.UpdateAsync(entity);

            var dto = _mapper.Map<MessageDto>(entity);
            var u = await _userRepo.GetByIdAsync(entity.UserId);
            dto.UserName = u?.UserName ?? "Unknown"; 
            return dto;
        }

        public async Task<Guid> DeleteMessageAsync(Guid messageId)
        {
            var entity = await _msgRepo.GetByIdAsync(messageId) ?? throw new KeyNotFoundException("Message not found");

            var chatId = entity.ChatId;
            await _msgRepo.DeleteAsync(messageId);
            return chatId;
        }
    }
}
