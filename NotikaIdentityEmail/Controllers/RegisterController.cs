using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using MimeKit;
using NotikaIdentityEmail.Entities;
using NotikaIdentityEmail.Models;
using System.Net.Mail;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace NotikaIdentityEmail.Controllers
{
    public class RegisterController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public RegisterController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(RegisterUserViewModel model)
        {
            Random rnd = new Random();
            int code = rnd.Next(100000, 1000000);
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Sifreler Uyusmuyor!");
                return View(model);
            }
            AppUser appUser = new AppUser()
            {
                Name = model.Name,
                Email = model.Email,
                Surname = model.Surname,
                UserName = model.UserName,
                ActivationCode = code,

            };
            var result = await _userManager.CreateAsync(appUser, model.Password);

            if (result.Succeeded)
            {
                // mail kodlari // yemi uukb anso zryu
                MimeMessage mimeMessage = new MimeMessage(); // MimeMessage uzerinden bir mail uretiyor

                MailboxAddress mailboxAddressFrom = new MailboxAddress("Admin", "bariscoskun441@gmail.com"); //mailin kimden gonderilecegi
                mimeMessage.From.Add(mailboxAddressFrom); //mimeMessage araciligiyla kimden gonderilecegini belirttik.

                MailboxAddress mailboxAddressTo = new MailboxAddress("User", model.Email); // mailin kime gonderilecegi belirtildi
                mimeMessage.To.Add(mailboxAddressTo);// mimeMessage araciligiyla kime gidecegini ekledik

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.TextBody = "Hesabinizi dogrulamak icin gerekli olan aktivasyon kodu " + code;
                mimeMessage.Body = bodyBuilder.ToMessageBody(); //mimeMessage araciligiyla mesaj icerigi gonderdik

                mimeMessage.Subject = "Notika Identity Aktivasyon Kodu";

                SmtpClient client = new SmtpClient(); //Mailkit client sinfindan cliet olustu(mail transfer protokolu)
                client.Connect("smtp.gmail.com", 587, false); // saglayici adi, port numarasi(turkiye icin),...
                client.Authenticate("bariscoskun441@gmail.com", "yemi uukb anso zryu");// Yetkilendirme icin bizim mail ve mailden aldigimiz aktivasyon kodumuz
                client.Send(mimeMessage);// mimeMessage ile degeri gonderdik
                client.Disconnect(true); // baglantiyi kes


                return RedirectToAction("UserActivation", "Activation");
            }
            foreach (var item in result.Errors)
            {
                ModelState.AddModelError("", item.Description);
            }
            return View(model);
        }
    }
}
