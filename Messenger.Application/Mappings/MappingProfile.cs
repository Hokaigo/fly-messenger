using AutoMapper;
using Messenger.Application.DTOs.Chats;
using Messenger.Application.DTOs.Profile;
using Messenger.Application.DTOs.Users;
using Messenger.Domain.Entities;
using Messenger.Domain.Enums;

namespace Messenger.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Chat, ChatDto>();

            CreateMap<Chat, ChatSummaryDto>().ForMember(d => d.ChatId, o => o.MapFrom(s => s.Id)).ForMember(d => d.OtherUserName, o => o.Ignore())
                .ForMember(d => d.LastMessage, o => o.Ignore()).ForMember(d => d.LastMessageTime, o => o.Ignore());

            CreateMap<Message, MessageDto>();

            CreateMap<SendMessageDto, Message>().ForMember(d => d.ChatId, o => o.MapFrom(s => s.ChatId))
                .ForMember(d => d.Text, o => o.MapFrom(s => (s.Text ?? "").Trim())).ForMember(d => d.DateSent, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.Type, o => o.MapFrom(s => s.File != null ? MessageType.File : MessageType.Text))
                .ForMember(d => d.Id, o => o.Ignore()).ForMember(d => d.UserId, o => o.Ignore()).ForMember(d => d.User, o => o.Ignore())
                .ForMember(d => d.FileUrl, o => o.Ignore()).ForMember(d => d.FileName, o => o.Ignore()).ForMember(d => d.FileType, o => o.Ignore())
                .ForMember(d => d.FileSize, o => o.Ignore());

            CreateMap<UserProfile, UserProfileDto>().ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId)).ForMember(d => d.UserName, o => o.Ignore())
                .ForMember(d => d.IsOnline, o => o.Ignore());

            CreateMap<UpdateProfileRequest, UserProfile>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<User, UserDto>();

            CreateMap<User, LoginResponse>().ForMember(d => d.UserId, o => o.MapFrom(s => s.Id));

            CreateMap<RegisterRequest, User>().ForMember(d => d.UserName, o => o.MapFrom(s => s.UserName)).ForMember(d => d.Email, o => o.MapFrom(s => s.Email))
                .ForMember(d => d.PasswordHash, o => o.Ignore()).ForMember(d => d.Id, o => o.Ignore());
        }
    }
}
