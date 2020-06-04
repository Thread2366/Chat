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
            List<ChatManager.Message> messages = manager.GetMessages();
            List<ChatMessage> chatMessages = messages.ConvertAll(m => new ChatMessage()
            {
                Sender = m.Sender.Username,
                Receiver = m.Receiver.Username,
                Message = m.Text,
                Timestamp = m.Timestamp
            });

            ChatModel model = new ChatModel()
            {
                Messages = chatMessages,
                Users = manager.GetAllUsers().Select(u => u.Username).ToList()
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Chat(string receiver, string message)
        {
            manager.SetUser(User.Identity.Name);
            manager.SendMessage(receiver, message);
            List<ChatManager.Message> messages = manager.GetMessages();
            List<ChatMessage> chatMessages = messages.ConvertAll(m => new ChatMessage()
            {
                Sender = m.Sender.Username,
                Receiver = m.Receiver.Username,
                Message = m.Text,
                Timestamp = m.Timestamp
            });

            ChatModel model = new ChatModel()
            {
                Messages = chatMessages,
                Users = manager.GetAllUsers().Select(u => u.Username).ToList()
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
    }
}