using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebChat.Models;

namespace WebChat.Controllers
{
    public class HomeController : Controller
    {
        private ChatManager.MessageManager manager = new ChatManager.MessageManager();

        // GET: Home
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(string login, string password)
        {
            if (manager.Register(login, password))
                return Redirect("/Home/Index");
            else
                return View();
        }

        [HttpPost]
        public ActionResult Login(string login, string password)
        {
            if (!manager.Authorize(login, password))
                return Redirect("/Home/Index");
            else
            {
                FormsAuthentication.SetAuthCookie(login, true);
                return Redirect("/Home/Chat");
            }
        }

        [Authorize]
        public ActionResult Chat()
        {
            manager.SetUser(User.Identity.Name);

            ChatModel model = new ChatModel()
            {
                Messages = ConvertToChatMessages(manager.GetMessages()),
                Users = manager.GetAllUsers().Select(u => u.Username).ToList()
            };

            return View(model);
        }

        [Authorize]
        public ActionResult GroupChat()
        {
            manager.SetUser(User.Identity.Name);

            ChatModel model = new ChatModel()
            {
                Messages = ConvertToChatMessages(manager.GetGroupMessages()),
                Users = manager.GetUserGroups().Select(g => g.Name).ToList()
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Chat(string receiver, string message)
        {
            manager.SetUser(User.Identity.Name);
            manager.SendMessage(receiver, message);

            ChatModel model = new ChatModel()
            {
                Messages = ConvertToChatMessages(manager.GetMessages()),
                Users = manager.GetAllUsers().Select(u => u.Username).ToList()
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult GroupChat(string receiver, string message)
        {
            manager.SetUser(User.Identity.Name);
            var group = manager
                .GetUserGroups()
                .FirstOrDefault(g => g.Name == receiver);
            manager.SendGroupMessage(group, message);

            ChatModel model = new ChatModel()
            {
                Messages = ConvertToChatMessages(manager.GetGroupMessages()),
                Users = manager.GetUserGroups().Select(g => g.Name).ToList()
            };

            return View(model);
        }


        [Authorize]
        [HttpPost]
        public ActionResult GetMessage(DateTime? start, DateTime? finish)
        {
            manager.SetUser(User.Identity.Name);
            List<ChatManager.Message> messages = manager.GetMessages(start.Value, finish.Value);
            List<ChatMessage> chatMessages = messages.ConvertAll(m => new ChatMessage()
            {
                Sender = m.Sender.Username,
                Receiver = m.Receiver.Username,
                Message = m.Text,
                Timestamp = m.Timestamp
            });

            return Json(chatMessages);
        }

        [Authorize]
        [HttpPost]
        public ActionResult GetGroupMessage(DateTime? start, DateTime? finish)
        {
            manager.SetUser(User.Identity.Name);
            List<ChatManager.GroupMessage> messages = manager.GetGroupMessages(start.Value, finish.Value);
            List<ChatMessage> groupMessages = messages.ConvertAll(m => new ChatMessage()
            {
                Sender = m.Sender.Username,
                Receiver = m.Group.Name,
                Message = m.Text,
                Timestamp = m.Timestamp
            });

            return Json(groupMessages);
        }

        private List<ChatMessage> ConvertToChatMessages(List<ChatManager.Message> messages) =>
            messages
            .ConvertAll(m => new ChatMessage()
            {
                Sender = m.Sender.Username,
                Receiver = m.Receiver.Username,
                Message = m.Text,
                Timestamp = m.Timestamp
            });

        private List<ChatMessage> ConvertToChatMessages(List<ChatManager.GroupMessage> messages) =>
            messages
            .ConvertAll(m => new ChatMessage()
            {
                Sender = m.Sender.Username,
                Receiver = m.Group.Name,
                Message = m.Text,
                Timestamp = m.Timestamp
            });
    }
}