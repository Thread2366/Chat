using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace ChatManager
{
    public class Participation
    {
        public int Id { get; set; }

        [Required]
        public virtual UserInfo User { get; set; }

        [Required]
        public virtual Group Group { get; set; }
        
        [Required]
        public DateTime LastRead { get; set; }
    }
}
