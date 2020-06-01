using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChatManager
{
    public class Message
    {
        public int Id { get; set; }
        public virtual UserInfo Sender { get; set; }
        public virtual UserInfo Receiver { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
