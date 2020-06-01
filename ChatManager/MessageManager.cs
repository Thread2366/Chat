using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web.Helpers;

namespace ChatManager
{
    public class MessageManager
    {
        private ChatContext context;
        private UserInfo currentUser;

        public MessageManager()
        {
            //Создание или подключение к базе данных
            context = new ChatContext();
        }

        public bool SetUser(string username)
        {
            currentUser = (from user in context.UserInfos
                           where user.Username == username
                           select user).SingleOrDefault();
            if (currentUser == null)
                return false;
            else
            {
                return true;
            }
        }

        public bool Register(string username, string password)
        {
            bool userExists = (from user in context.UserInfos
                               where user.Username == username
                               select user).Any();
            //userExists = context.UserInfos.Any(user => user.Username == username);

            if (userExists)
                return false;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return false;
            context.UserInfos.Add(new UserInfo()
            {
                Username = username,
                PasswordHash = Crypto.HashPassword(password)
            });
            context.SaveChanges();
            return true;
        }

        public bool Authorize(string username, string password)
        {
            currentUser = (from user in context.UserInfos
                           where user.Username == username
                           select user).SingleOrDefault();
            if (currentUser == null)
                return false;
            if( Crypto.VerifyHashedPassword(currentUser.PasswordHash, password))
            {
                return true;
            }
            else
            {
                currentUser = null;
                return false;
            }
        }

        public bool SendMessage(string username, string messageText)
        {
            if (currentUser == null)
                return false;
            UserInfo receiver = context.UserInfos.SingleOrDefault(user => user.Username == username);
            if (receiver == null || string.IsNullOrWhiteSpace(messageText))
                return false;
            context.Messages.Add(new Message()
            {
                Sender = currentUser,
                Receiver = receiver,
                Text = messageText,
                Timestamp = DateTime.Now
            });
            context.SaveChanges();
            return true;
        }

        public List<Message> GetMessages()
        {
            if (currentUser == null)
                return null;
            IQueryable<Message> messagesQuery = from message in context.Messages
                                                where message.Sender.Id == currentUser.Id || message.Receiver.Id == currentUser.Id
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
            if (currentUser == null)
                return null;
            IQueryable<Message> messagesQuery = from message in context.Messages
                                                where (message.Sender.Id == currentUser.Id || message.Receiver.Id == currentUser.Id) && message.Timestamp>=start && message.Timestamp<finish
                                                orderby message.Timestamp
                                                select message;
            //IQueryable<Message> messagesQuery = context.Messages.
            //    Where(m => m.Sender == currentUser || m.Reveiver == currentUser)
            //    .OrderBy(ms => ms.Timestamp);
            //IQueryable<Message> messagesQuery = context.Messages.Where(message => message.Sender == currentUser || message.Reveiver == currentUser);

            return messagesQuery.ToList();
        }

        public List<string> GetAllUsers()
        {
            if (currentUser == null)
                return null;
            IQueryable<string> usersQuery = from user in context.UserInfos
                                            where user.Id != currentUser.Id
                                            select user.Username;
            return usersQuery.ToList();
        }


        //Далее функции для работы с группами

        public bool CreateGroup(string groupName, GroupAccessMode accessMode)
        {
            if (currentUser == null) return false;
            var group = new Group()
            {
                Name = groupName,
                AccessMode = accessMode,
                Administrator = currentUser
            };
            context.Groups.Add(group);
            context.Participations.Add(new Participation()
            {
                Group = group,
                LastRead = DateTime.Now,
                User = currentUser
            });
            context.SaveChanges();
            return true;
        }

        public bool EnterGroup(Group group)
        {
            if (currentUser == null) return false;
            context.Participations.Add(new Participation()
            {
                Group = group,
                User = currentUser,
                LastRead = DateTime.Now
            });
            context.SaveChanges();
            return true;
        }

        public bool SendGroupMessage(Group group, string text)
        {
            if (currentUser == null) return false;
            var groupMember = context.Participations
                .SingleOrDefault(p => p.Group.Id == group.Id && p.User.Id == currentUser.Id);
            if (groupMember == null) return false;
            context.GroupMessages.Add(new GroupMessage()
            {
                Text = text,
                Group = group,
                Sender = currentUser,
                Timestamp = DateTime.Now
            });
            context.SaveChanges();
            return true;
        }

        public List<GroupMessage> GetGroupMessages(Group group, bool unread)
        {
            if (currentUser == null) return null;
            var groupMember = context.Participations
                .SingleOrDefault(p => p.Group.Id == group.Id && p.User.Id == currentUser.Id);
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

        public List<Group> GetPublicGroups()
        {
            if (currentUser == null) return null;
            return context.Groups
                .Where(grp => grp.AccessMode == GroupAccessMode.Public)
                .ToList();
        }

        public List<Group> GetUserGroups()
        {
            if (currentUser == null) return null;
            return context.Participations
                .Where(p => p.User.Id == currentUser.Id)
                .Select(p => p.Group)
                .ToList();
        }
    }
}
