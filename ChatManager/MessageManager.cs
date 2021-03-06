﻿using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Data;
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
            var currentUser = (from user in context.UserInfos
                           where user.Username == username
                           select user).SingleOrDefault();
            if (currentUser == null)
                return false;
            if (Crypto.VerifyHashedPassword(currentUser.PasswordHash, password))
                return true;
            else
            {
                currentUser = null;
                return false;
            }
        }

        public bool SendMessage(string username, string messageText)
        {
            if (currentUser == null) return false;
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
            if (currentUser == null) return null;
            IQueryable<Message> messagesQuery = from message in context.Messages
                                                where message.Sender.Id == currentUser.Id 
                                                || message.Receiver.Id == currentUser.Id
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
            if (currentUser == null) return null;
            IQueryable<Message> messagesQuery = from message in context.Messages
                                                where (message.Sender.Id == currentUser.Id 
                                                || message.Receiver.Id == currentUser.Id) 
                                                && message.Timestamp >= start 
                                                && message.Timestamp < finish
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
            if (currentUser == null) return null;
            IQueryable<UserInfo> usersQuery = from user in context.UserInfos
                                              where user.Id != currentUser.Id
                                              select user;
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
            var groupMember = context.Participations
                .SingleOrDefault(p => p.Group.Id == group.Id && p.User.Id == currentUser.Id);
            if (groupMember != null) return false;
            context.Participations.Add(new Participation()
            {
                Group = group,
                User = currentUser,
                LastRead = DateTime.Now
            });
            context.SaveChanges();
            return true;
        }

        public bool LeaveGroup(Group group)
        {
            if (currentUser == null) return false;
            if (group.Administrator.Id == currentUser.Id)
            {
                var anotherMember = context.Participations
                    .FirstOrDefault(p => p.Group.Id == group.Id && p.User.Id != currentUser.Id);
                if (anotherMember == null)
                    context.Groups.Remove(group);
                else group.Administrator = anotherMember.User;
            }
            var groupMember = context.Participations
                .SingleOrDefault(p => p.Group.Id == group.Id && p.User.Id == currentUser.Id);
            if (groupMember == null) return false;
            context.Participations.Remove(groupMember);
            context.SaveChanges();
            return true;
        }

        public bool MakeAnAdministrator(Group group, UserInfo user)
        {
            if (currentUser == null) return false;
            if (group.Administrator.Id != currentUser.Id) return false;
            if (context.Participations
                .SingleOrDefault(p => p.Group.Id == group.Id && p.User.Id == user.Id) == null)
                return false;
            group.Administrator = user;
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

        public List<GroupMessage> GetGroupMessages()
        {
            if (currentUser == null) return null;
            var groups = GetUserGroups().Select(g => g.Id);
            var messages = context.GroupMessages
                .Where(msg => groups.Contains(msg.Group.Id))
                .ToList();
            return messages;
        }

        public List<GroupMessage> GetGroupMessages(DateTime start, DateTime finish)
        {
            if (currentUser == null) return null;
            var groups = GetUserGroups().Select(g => g.Id);
            var messages = context.GroupMessages
                .Where(msg => groups.Contains(msg.Group.Id))
                .Where(msg => msg.Timestamp >= start && msg.Timestamp < finish)
                .ToList();
            return messages;
        }

        public List<Group> GetAvailableGroups()
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

        public bool DeleteGroup(Group group)
        {
            if (currentUser == null) return false;
            if (group.Administrator.Id != currentUser.Id) return false;
            context.Groups.Remove(group);
            context.SaveChanges();
            return true;
        }

        public bool DeleteMessage(GroupMessage message)
        {
            if (currentUser == null) return false;
            if (message.Group.Administrator.Id != currentUser.Id) return false;
            context.GroupMessages.Remove(message);
            context.SaveChanges();
            return true;
        }

        public bool AddToGroup(Group group, UserInfo user)
        {
            if (currentUser == null) return false;
            if (user.Id == currentUser.Id) return false;
            if (group.Administrator.Id != currentUser.Id)
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
