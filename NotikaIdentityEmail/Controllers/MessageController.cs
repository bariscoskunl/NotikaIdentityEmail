using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NotikaIdentityEmail.Context;
using NotikaIdentityEmail.Entities;
using NotikaIdentityEmail.Models.MessageViewModels;

namespace NotikaIdentityEmail.Controllers
{
    [Authorize]
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
            var values = (from m in _context.Messages // mesaj tablosundaki degerlere m uzerinden ulasacagiz
                join u in _context.Users // user tablosundaki degerlere u uzerinden ulasasacagiz
                on m.SenderEmail equals u.Email into userGroup
                from sender in userGroup.DefaultIfEmpty() 

                join c in _context.Categories
                on m.CategoryId equals c.CategoryId into categoryGroup
                from category in categoryGroup.DefaultIfEmpty()

                where m.ReceiverEmail == user.Email  // giris yapan kullanici mailine esit olan degerden
                select new MessageWithSenderInfoViewModel  // bunun icine atiyoruz
                { 
                    MessageId = m.MessageId,  
                    MessageDetail = m.MessageDetail,
                    Subject = m.Subject,
                    SendDate = m.SendDate,
                    SenderEmail = m.SenderEmail,
                    SenderName = sender.Name != null ? sender.Name : "Bilinmeyen",
                    SenderSurname = sender.Surname != null ? sender.Surname : "Kullanici",
                    CategoryName = category != null ? category.CategoryName: "Kategori yok"
                }).ToList();
            var userMail = await _userManager.FindByNameAsync(User.Identity.Name);
            ViewBag.SenderEmail = user.Email;
            return View(values);
        }
        public async Task<IActionResult> Sendbox()
        {

            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null) return RedirectToAction("UserLogin", "Login");
            var values = (from m in _context.Messages 
                          join u in _context.Users
                          on m.ReceiverEmail.Trim().ToLower() equals u.Email.Trim().ToLower() into userGroup
                          from reciever in userGroup.DefaultIfEmpty()

                          join c in _context.Categories
                          on m.CategoryId equals c.CategoryId into categoryGroup
                          from category in categoryGroup.DefaultIfEmpty()

                          where m.SenderEmail == user.Email  
                          select new MessageWithRecieverInfoViewModel 
                          {
                              MessageId = m.MessageId,
                              MessageDetail = m.MessageDetail,
                              Subject = m.Subject,
                              SendDate = m.SendDate,
                              RecieverEmail = m.ReceiverEmail,
                              RecieverName = reciever != null ? reciever.Name : "Bilinmeyen",
                              RecieverSurname = reciever != null ? reciever.Surname : "Kullanıcı",
                              CategoryName = category != null ? category.CategoryName : "Kategori yok"
                          }).ToList();
            var userMail = await _userManager.FindByNameAsync(User.Identity.Name);
            ViewBag.SenderEmail = user.Email;
            return View(values);
        }
        public IActionResult MessageDetail(int id)
        {
            var value = _context.Messages.Where(x => x.MessageId == id).FirstOrDefault();
            return View(value);
        }
        [HttpGet]
        public IActionResult ComposeMessage()
        {
            var categories = _context.Categories.ToList();

            ViewBag.v = categories.Select(c => new SelectListItem
            {
                Text = c.CategoryName,
                Value = c.CategoryId.ToString()

            }).ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ComposeMessage(Message message)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            message.SenderEmail = user.Email;
            message.SendDate = DateTime.Now;
            message.IsRead = false;
            _context.Messages.Add(message);
            _context.SaveChanges();
            return RedirectToAction("Sendbox");
        }

    }
}
