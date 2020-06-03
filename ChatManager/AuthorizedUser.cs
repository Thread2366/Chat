using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChatManager
{
    public class AuthorizedUser
    {
        private ChatContext context;

        public UserInfo UserInfo { get; }

        public AuthorizedUser(ChatContext context, UserInfo user)
        {
            this.context = context;
            UserInfo = user;
        }

        public bool SendMessage(string username, string messageText)
        {
            UserInfo receiver = context.UserInfos.SingleOrDefault(user => user.Username == username);
            if (receiver == null || string.IsNullOrWhiteSpace(messageText))
                return false;
            context.Messages.Add(new Message()
            {
                Sender = UserInfo,
                Receiver = receiver,
                Text = messageText,
                Timestamp = DateTime.Now
            });
            context.SaveChanges();
            return true;
        }

        public List<Message> GetMessages()
        {
            IQueryable<Message> messagesQuery = from message in context.Messages
                                                where message.Sender.Id == UserInfo.Id || message.Receiver.Id == UserInfo.Id
                                                orderby message.Timestamp
                                                select message;
            //IQueryable<Message> messagesQuery = context.Messages.
            //    Where(m => m.Sender == currentUser || m.Reveiver == currentUser)
            //    .OrderBy(ms => ms.Timestamp);
            //IQueryable<Message> messagesQuery = context.Messages.Where(message => message.Sender == currentUser || message.Reveiver == currentUser);

            return messagesQuery.ToList();
        }

        public List<Message> GetMessages(DateTime start, DateTime finish)
        {
            IQueryable<Message> messagesQuery = from message in context.Messages
                                                where (message.Sender.Id == UserInfo.Id || message.Receiver.Id == UserInfo.Id) && message.Timestamp >= start && message.Timestamp < finish
                                                orderby message.Timestamp
                                                select message;
            //IQueryable<Message> messagesQuery = context.Messages.
            //    Where(m => m.Sender == currentUser || m.Reveiver == currentUser)
            //    .OrderBy(ms => ms.Timestamp);
            //IQueryable<Message> messagesQuery = context.Messages.Where(message => message.Sender == currentUser || message.Reveiver == currentUser);

            return messagesQuery.ToList();
        }

        public List<UserInfo> GetAllUsers()
        {
            IQueryable<UserInfo> usersQuery = from user in context.UserInfos
                                            where user.Id != UserInfo.Id
                                            select user;
            return usersQuery.ToList();
        }


        //Далее функции для работы с группами

        public bool CreateGroup(string groupName, GroupAccessMode accessMode)
        {
            var group = new Group()
            {
                Name = groupName,
                AccessMode = accessMode,
                Administrator = UserInfo
            };
            context.Groups.Add(group);
            context.Participations.Add(new Participation()
            {
                Group = group,
                LastRead = DateTime.Now,
                User = UserInfo
            });
            context.SaveChanges();
            return true;
        }

        public bool EnterGroup(Group group)
        {
            var groupMember = context.Participations
                .SingleOrDefault(p => p.Group.Id == group.Id && p.User.Id == UserInfo.Id);
            if (groupMember != null) return false;
            context.Participations.Add(new Participation()
            {
                Group = group,
                User = UserInfo,
                LastRead = DateTime.Now
            });
            context.SaveChanges();
            return true;
        }

        public bool LeaveGroup(Group group)
        {
            if (group.Administrator.Id == UserInfo.Id)
            {
                var anotherMember = context.Participations
                    .FirstOrDefault(p => p.Group.Id == group.Id && p.User.Id != UserInfo.Id);
                if (anotherMember == null)
                    context.Groups.Remove(group);
                else group.Administrator = anotherMember.User;
            }
            var groupMember = context.Participations
                .SingleOrDefault(p => p.Group.Id == group.Id && p.User.Id == UserInfo.Id);
            if (groupMember == null) return false;
            context.Participations.Remove(groupMember);
            context.SaveChanges();
            return true;
        }

        public bool MakeAnAdministrator(Group group, UserInfo user)
        {
            if (group.Administrator.Id != UserInfo.Id) return false;
            if (context.Participations
                .SingleOrDefault(p => p.Group.Id == group.Id && p.User.Id == user.Id) == null)
                return false;
            group.Administrator = user;
            context.SaveChanges();
            return true;
        }

        public bool SendGroupMessage(Group group, string text)
        {
            var groupMember = context.Participations
                .SingleOrDefault(p => p.Group.Id == group.Id && p.User.Id == UserInfo.Id);
            if (groupMember == null) return false;
            context.GroupMessages.Add(new GroupMessage()
            {
                Text = text,
                Group = group,
                Sender = UserInfo,
                Timestamp = DateTime.Now
            });
            context.SaveChanges();
            return true;
        }

        public List<GroupMessage> GetGroupMessages(Group group, bool unread)
        {
            var groupMember = context.Participations
                .SingleOrDefault(p => p.Group.Id == group.Id && p.User.Id == UserInfo.Id);
            if (groupMember == null) return null;
            var messages = context.GroupMessages
                .Where(msg => msg.Group.Id == group.Id);

            var result = unread ?
                messages.Where(msg => msg.Timestamp > groupMember.LastRead).ToList() :
                messages.ToList();

            groupMember.LastRead = DateTime.Now;
            context.SaveChanges();
            return result;
        }

        public List<Group> GetAvailableGroups()
        {
            return context.Groups
                .Where(grp => grp.AccessMode == GroupAccessMode.Public)
                .ToList();
        }

        public List<Group> GetUserGroups()
        {
            return context.Participations
                .Where(p => p.User.Id == UserInfo.Id)
                .Select(p => p.Group)
                .ToList();
        }

        public bool DeleteGroup(Group group)
        {
            if (group.Administrator.Id != UserInfo.Id) return false;
            context.Groups.Remove(group);
            context.SaveChanges();
            return true;
        }

        public bool DeleteMessage(GroupMessage message)
        {
            if (message.Group.Administrator.Id != UserInfo.Id) return false;
            context.GroupMessages.Remove(message);
            context.SaveChanges();
            return true;
        }

        public bool AddToGroup(Group group, UserInfo user)
        {
            if (user.Id == UserInfo.Id) return false;
            if (group.Administrator.Id != UserInfo.Id)
                return false;
            context.Participations.Add(new Participation()
            {
                Group = group,
                User = user,
                LastRead = DateTime.Now
            });
            context.SaveChanges();
            return true;
        }
    }
}
