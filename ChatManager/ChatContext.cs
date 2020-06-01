using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace ChatManager
{
    public class ChatContext: DbContext
    {
        public DbSet<UserInfo> UserInfos { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<GroupMessage> GroupMessages { get; set; }
        public DbSet<Participation> Participations { get; set; }
        public DbSet<Group> Groups { get; set; }
    }
}
