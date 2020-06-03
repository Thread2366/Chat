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

        public MessageManager()
        {
            //Создание или подключение к базе данных
            context = new ChatContext();
        }

        //public bool SetUser(string username)
        //{
        //    currentUser = (from user in context.UserInfos
        //                   where user.Username == username
        //                   select user).SingleOrDefault();
        //    if (currentUser == null)
        //        return false;
        //    else
        //    {
        //        return true;
        //    }
        //}

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

        public AuthorizedUser Authorize(string username, string password)
        {
            var currentUser = (from user in context.UserInfos
                           where user.Username == username
                           select user).SingleOrDefault();
            if (currentUser == null)
                return null;
            if (Crypto.VerifyHashedPassword(currentUser.PasswordHash, password))
                return new AuthorizedUser(context, currentUser);
            else return null;
        }
    }
}
