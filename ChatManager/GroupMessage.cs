using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace ChatManager
{
    public class GroupMessage
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        public virtual UserInfo Sender { get; set; }

        [Required]
        public virtual Group Group { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }
    }
}
