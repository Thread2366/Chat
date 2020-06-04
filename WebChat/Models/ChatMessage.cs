using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebChat.Models
{
    public class ChatMessage
    {
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
    }
}