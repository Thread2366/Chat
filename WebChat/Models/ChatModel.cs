using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebChat.Models
{
    public class ChatModel
    {
        public List<ChatMessage> Messages { get; set; }
        public List<string> Users { get; set; }
    }
}