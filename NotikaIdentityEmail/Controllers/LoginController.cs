using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotikaIdentityEmail.Context;
using NotikaIdentityEmail.Entities;
using NotikaIdentityEmail.Models.IdentityModels;

namespace NotikaIdentityEmail.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly EmailContext _context;

        public LoginController(SignInManager<AppUser> signInManager, EmailContext context)
        {
            _signInManager = signInManager;
            _context = context;
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

            var value = _context.Users.Where(x => x.UserName == model.UserName).FirstOrDefault();
            if (value.EmailConfirmed == true)
            {
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, true, true);
                if (result.Succeeded)
                {
                    return RedirectToAction("Profile", "EditProfile");
                }
            }            
            ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı!");
            return View(model);
        }
    }
}
