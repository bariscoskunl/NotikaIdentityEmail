using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using NotikaIdentityEmail.Entities;
using NotikaIdentityEmail.Models.EmailModels;
using NotikaIdentityEmail.Models.FotgetPasswordModels;

namespace NotikaIdentityEmail.Controllers
{
    [AllowAnonymous]
    public class PasswordChangeController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly EmailSettingsModel _emailSettings;

        public PasswordChangeController(UserManager<AppUser> userManager, IOptions<EmailSettingsModel> emailSettings)
        {
            _userManager = userManager;
            _emailSettings = emailSettings.Value;
        }

        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel forgetPasswordViewModel)
        {
            var user = await _userManager.FindByEmailAsync(forgetPasswordViewModel.Email);
            if (user == null)
            {
                return View();
            }

            string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var passwordResetTokenLink = Url.Action("ResetPassword", "PasswordChange", new
            {
                userId = user.Id,
                token = passwordResetToken
            }, HttpContext.Request.Scheme);

            MimeMessage mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            mimeMessage.To.Add(new MailboxAddress("User", forgetPasswordViewModel.Email));

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = passwordResetTokenLink;
            mimeMessage.Body = bodyBuilder.ToMessageBody();
            mimeMessage.Subject = "Şifre Sıfırlama Talebi";

            using (SmtpClient client = new SmtpClient())
            {
                await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.Password);
                await client.SendAsync(mimeMessage);
                await client.DisconnectAsync(true);
            }

            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction("UserLogin", "Login");
            }
            TempData["userId"] = userId;
            TempData["token"] = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
        {
            var userId = TempData["userId"]?.ToString();
            var token = TempData["token"]?.ToString();

            if (userId == null || token == null)
            {
                return RedirectToAction("UserLogin", "Login");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("UserLogin", "Login");
            }

            if (resetPasswordViewModel.Password == resetPasswordViewModel.ConfirmPassword)
            {
                var result = await _userManager.ResetPasswordAsync(user, token, resetPasswordViewModel.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("UserLogin", "Login");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            else
            {
                ModelState.AddModelError("", "Şifreler uyuşmuyor.");
            }

            TempData.Keep("userId");
            TempData.Keep("token");

            return View(resetPasswordViewModel);
        }
    }
}