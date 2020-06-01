using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace ChatManager
{
    public class UserInfo
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }


        public virtual IQueryable<Group> Groups { get; set; }
    }
}
