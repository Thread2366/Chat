using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace ChatManager
{
    public class Group
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual UserInfo Administrator { get; set; }

        [Required]
        public GroupAccessMode AccessMode { get; set; }
    }
}
