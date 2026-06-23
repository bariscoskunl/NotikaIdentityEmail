using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotikaIdentityEmail.Context;
using NotikaIdentityEmail.Entities;
using NotikaIdentityEmail.Models.IdentityModels;
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
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> UserLogin(UserLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var value = _context.Users.Where(x => x.UserName == model.UserName || x.Email == model.UserName).FirstOrDefault();
            if (value.EmailConfirmed == true)
            {
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, true, true);
                if (result.Succeeded)
                {
                    return RedirectToAction("EditProfile", "Profile");
                }
            }            
            ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı!");
            return View(model);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UserLogout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("UserLogin", "Login");
        }

        [HttpPost]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallBack","Login", new {returnUrl}); // Burada returnUrl parametresi ile yönlendirme yapılacak sayfa bilgisi alınır.
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl); // Burada provider parametresi ile hangi dış sağlayıcı kullanılacağı belirlenir ve redirectUrl ile yönlendirme yapılacak sayfa bilgisi alınır.
            return Challenge(properties, provider); // Challenge metodu ile dış sağlayıcıya yönlendirme yapılır.
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
                // Kullanıcı sistemde yoksa yeni oluştur
                user = new AppUser()
                {
                    UserName = email,
                    Email = email,
                    Name = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "Google",
                    Surname = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "User",
                    EmailConfirmed = true // Google'dan geldiği için maili onaylı sayıyoruz
                };
                var createResult = await _userManager.CreateAsync(user); 
                if (!createResult.Succeeded)
                {
                    return RedirectToAction("UserLogin");
                }
                isNewUser = true;
            }

            // Google hesabı daha önce bu kullanıcıya bağlanmamışsa bağla
            var logins = await _userManager.GetLoginsAsync(user);
            if (!logins.Any(l => l.LoginProvider == info.LoginProvider && l.ProviderKey == info.ProviderKey))
            {
                await _userManager.AddLoginAsync(user, info); 
            }

            // Kullanıcının rolü yoksa 403 almaması için "User" rolünü ata
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Count == 0)
            {
                await _userManager.AddToRoleAsync(user, "User"); 
            }

            // Giriş yap
            await _signInManager.SignInAsync(user, isPersistent: false); 
            
            // Eğer yeni kullanıcıysa eksik bilgilerini (UserName vb.) tamamlaması için Profile yönlendir
            if (isNewUser)
            {
                return RedirectToAction("EditProfile", "Profile");
            }
            
            // Eski kullanıcıysa direkt Inbox'a gitsin
            return RedirectToAction("Inbox", "Message");
        }
    }
}
