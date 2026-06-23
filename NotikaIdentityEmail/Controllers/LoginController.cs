using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NotikaIdentityEmail.Context;
using NotikaIdentityEmail.Entities;
using NotikaIdentityEmail.Models.IdentityModels;
using NotikaIdentityEmail.Models.JwtModels;
using System.Security.Claims;

namespace NotikaIdentityEmail.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly EmailContext _context;
        private readonly UserManager<AppUser> _userManager;

        public LoginController(SignInManager<AppUser> signInManager, EmailContext context, UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _context = context;
            _userManager = userManager;
        }
        [HttpGet]
        public IActionResult UserLogin()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> UserLogin(UserLoginViewModel model)
        {            
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var value = _context.Users.FirstOrDefault(x => x.UserName == model.UserName || x.Email == model.UserName);

            if (value == null)
            {
                ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı!");
                return View(model);
            }

            if (!value.EmailConfirmed)
            {
                ModelState.AddModelError("", "E-Mail adresiniz henüz onaylanmamış.");
                return View(model);
            }
            if (!value.IsActive)
            {
                ModelState.AddModelError("", "Kullanıcı pasif durumda, giriş yapamaz!");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, true, true);
            if (result.Succeeded)
            {
                return RedirectToAction("EditProfile", "Profile");
            }

            ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı!");
            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> UserLogout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallBack", "Login", new { returnUrl }); 
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl); 
            return Challenge(properties, provider); 
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallBack(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= Url.Content("~/");
            if (remoteError != null)
            {
                ModelState.AddModelError("", "Dış sağlayıcıdan hata alındı.");
                return View("UserLogin");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction("UserLogin");
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == email);
            bool isNewUser = false;

            if (user == null)
            {
                user = new AppUser()
                {
                    UserName = email,
                    Email = email,
                    Name = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "Google",
                    Surname = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "User",
                    EmailConfirmed = true 
                };
                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return RedirectToAction("UserLogin");
                }
                isNewUser = true;
            }

            var logins = await _userManager.GetLoginsAsync(user);
            if (!logins.Any(l => l.LoginProvider == info.LoginProvider && l.ProviderKey == info.ProviderKey))
            {
                await _userManager.AddLoginAsync(user, info);
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Count == 0)
            {
                await _userManager.AddToRoleAsync(user, "User");
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            if (isNewUser)
            {
                return RedirectToAction("EditProfile", "Profile");
            }

            return RedirectToAction("Inbox", "Message");
        }
    }
}
