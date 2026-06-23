using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using NotikaIdentityEmail.Entities;
using NotikaIdentityEmail.Models.EmailModels;
using NotikaIdentityEmail.Models.IdentityModels;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace NotikaIdentityEmail.Controllers
{
    [AllowAnonymous]
    public class RegisterController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly EmailSettingsModel _emailSettings;

        public RegisterController(UserManager<AppUser> userManager, IOptions<EmailSettingsModel> emailSettings)
        {
            _userManager = userManager;
            _emailSettings = emailSettings.Value;
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
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
                var registeredUser = await _userManager.FindByEmailAsync(model.Email);
                if (registeredUser != null)
                {
                    registeredUser.ActivationCode = code;
                    await _userManager.UpdateAsync(registeredUser);
                }

                // mail kodlari
                MimeMessage mimeMessage = new MimeMessage();

                MailboxAddress mailboxAddressFrom = new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail);
                mimeMessage.From.Add(mailboxAddressFrom);

                MailboxAddress mailboxAddressTo = new MailboxAddress("User", model.Email);
                mimeMessage.To.Add(mailboxAddressTo);

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.TextBody = "Hesabinizi dogrulamak icin gerekli olan aktivasyon kodu " + code;
                mimeMessage.Body = bodyBuilder.ToMessageBody();

                mimeMessage.Subject = "Notika Identity Aktivasyon Kodu";

                using (SmtpClient client = new SmtpClient())
                {
                    await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.Password);
                    await client.SendAsync(mimeMessage);
                    await client.DisconnectAsync(true);
                }

                TempData["EmailMove"] = model.Email;

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