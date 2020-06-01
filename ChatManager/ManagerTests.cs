using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ChatManager
{
    [TestFixture]
    public static class ManagerTests
    {
        static MessageManager manager = new MessageManager();

        [Test]
        public static void CreateGroupTest()
        {
            manager.Register("username1", "password1");
            manager.Authorize("username1", "password1");
            manager.CreateGroup("group1", GroupAccessMode.Public);
        }

        [Test]
        public static void EnterGroupTest()
        {
            manager.Register("username2", "password2");
            manager.Authorize("username2", "password2");
            var group = manager.GetPublicGroups().First(g => g.Name == "group1");
            manager.EnterGroup(group);
        }

        [Test]
        public static void SendGroupMessageTest()
        {
            manager.Register("username2", "password2");
            manager.Authorize("username2", "password2");
            var group = manager.GetPublicGroups().First(g => g.Name == "group1");
            manager.SendGroupMessage(group, "some text");
        }

        [Test]
        public static void GetGroupMessagesTest()
        {
            manager.Authorize("username2", "password2");
            var group = manager.GetPublicGroups().First(g => g.Name == "group1");
            var messages = manager.GetGroupMessages(group, true);
            messages = manager.GetGroupMessages(group, true);
            messages = manager.GetGroupMessages(group, false);
        }

        [Test]
        public static void GetUserGroupsTest()
        {
            manager.Authorize("username2", "password2");
            manager.CreateGroup("group2", GroupAccessMode.Private);
            var groups = manager.GetUserGroups();
        }
    }
}
