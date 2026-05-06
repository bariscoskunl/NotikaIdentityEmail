using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotikaIdentityEmail.Context;

namespace NotikaIdentityEmail.Controllers
{
    public class MessageController : Controller
    {
        private readonly EmailContext _context;

        public MessageController(EmailContext context)
        {
            _context = context;
        }
        public IActionResult Inbox()
        {
            var values = _context.Messages.Where(x => x.ReceiverEmail == "ali@gmail.com").ToList();
            return View(values);
        }
        public IActionResult Sendbox()
        {
            var values = _context.Messages.Where(x => x.SenderEmail == "ali@gmail.com").ToList();
            return View(values);
        }
        public IActionResult MessageDetail()
        {
            var value = _context.Messages.Where(x => x.MessageId == 7).FirstOrDefault();
            return View(value);
        }
        public IActionResult ComposeMessage()
        {          
            return View();
        }
        
    }
}
