using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotikaIdentityEmail.Context;
using NotikaIdentityEmail.Entities;
using NotikaIdentityEmail.Models;

namespace NotikaIdentityEmail.Controllers
{
    public class MessageController : Controller
    {
        private readonly EmailContext _context;
        private readonly UserManager<AppUser> _userManager;

        public MessageController(EmailContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Inbox()
        {
            
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            //var values = _context.Messages.Where(x => x.ReceiverEmail == user.Email).ToList();
            var values = (from m in _context.Messages // mesaj tablosundaki degerlere m uzerinden ulasacagiz
                join u in _context.Users // user tablosundaki degerlere u uzerinden ulasasacagiz
                on m.SenderEmail equals u.Email into userGroup
                from sender in userGroup.DefaultIfEmpty()
                where m.ReceiverEmail == user.Email  // giris yapan kullanici mailine esit olan degerden
                select new MessageWithSenderInfoViewModel  // bunun icine atiyoruz
                { 
                    MessageId = m.MessageId,  
                    MessageDetail = m.MessageDetail,
                    Subject = m.Subject,
                    SendDate = m.SendDate,
                    SenderEmail = m.SenderEmail,
                    SenderName = sender.Name != null ? sender.Name : "Bilinmeyen",
                    SenderSurname = sender.Surname != null ? sender.Surname : "Kullanici"
                }).ToList();
            var userMail = await _userManager.FindByNameAsync(User.Identity.Name);
            ViewBag.SenderEmail = user.Email;
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
