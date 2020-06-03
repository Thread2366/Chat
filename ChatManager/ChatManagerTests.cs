using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;

namespace ChatManager
{
    [TestFixture]
    public static class ChatManagerTests
    {
        static MessageManager manager = new MessageManager();

        [Test]
        public static void CreateGroupTest()
        {
            manager.Register("username1", "password1");
            var user = manager.Authorize("username1", "password1");
            user.CreateGroup("group1", GroupAccessMode.Public);
        }

        [Test]
        public static void EnterGroupTest()
        {
            //manager.Register("username2", "password2");
            var user = manager.Authorize("username2", "password2");
            var group = user.GetAvailableGroups().First(g => g.Name == "group1");
            user.EnterGroup(group);
            user.EnterGroup(group);
        }

        [Test]
        public static void SendGroupMessageTest()
        {
            var user = manager.Authorize("username2", "password2");
            var group = user.GetAvailableGroups().First(g => g.Name == "group1");
            user.SendGroupMessage(group, "some text");
        }

        [Test]
        public static void GetGroupMessagesTest()
        {
            var user = manager.Authorize("username2", "password2");
            var group = user.GetUserGroups().First(g => g.Name == "group1");
            var messages = user.GetGroupMessages(group, true);
            messages = user.GetGroupMessages(group, true);
            messages = user.GetGroupMessages(group, false);
        }

        [Test]
        public static void GetUserGroupsTest()
        {
            var user = manager.Authorize("username2", "password2");
            user.CreateGroup("group2", GroupAccessMode.Private);
            var groups = user.GetUserGroups();
        }

        [Test]
        public static void DeleteGroupTest()
        {
            var user = manager.Authorize("username2", "password2");
            var group = user.GetUserGroups().First(g => g.Name == "group2");
            user.DeleteGroup(group);
        }

        [Test]
        public static void LeaveGroupTest()
        {
            var user = manager.Authorize("username2", "password2");
            var group = user.GetUserGroups().First(g => g.Name == "group1");
            user.LeaveGroup(group);
        }

        [Test]
        public static void MakeAnAdministratorTest()
        {
            var user = manager.Authorize("username1", "password1");
            var group = user.GetUserGroups().First(g => g.Id == 3);
            var anotherUser = user.GetAllUsers().First(u => u.Id == 2);
            user.MakeAnAdministrator(group, anotherUser);
        }

        [Test]
        public static void AddToGroupTest()
        {
            var user = manager.Authorize("username2", "password2");
            var group = user.GetUserGroups().First(g => g.Id == 5);
            var anotherUser = user.GetAllUsers().First(u => u.Id == 1);
            user.AddToGroup(group, anotherUser);
        }
    }
}
