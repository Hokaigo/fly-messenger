using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Application.DTOs.Chats
{
    public class SendMessageDto
    {
        public Guid ChatId { get; set; }

        public string? Text { get; set; }

        public IFormFile? File { get; set; }
    }

}
