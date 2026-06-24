using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotikaIdentityEmail.Context;

namespace NotikaIdentityEmail.Controllers
{
    public class ActivationController : Controller
    {
        private readonly EmailContext _context;

        public ActivationController(EmailContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult UserActivation()
        {
            if (TempData["EmailMove"] != null)
            {
                TempData["Move"] = TempData["EmailMove"];
            }
            return View();
        }

        [HttpPost]
        public IActionResult UserActivation(int userCodeParamater)
        {
            if (TempData.Peek("Move") == null)
            {
                return RedirectToAction("CreateUser", "Register");
            }

            string email = TempData["Move"].ToString();

            var user = _context.Users.FirstOrDefault(x => x.Email == email);

            if (user == null)
            {
                ViewBag.ErrorMessage = "Kullanıcı bulunamadı.";
                return View();
            }

            if (userCodeParamater == user.ActivationCode)
            {
                user.EmailConfirmed = true;
                _context.SaveChanges();
                TempData.Remove("Move");
                return RedirectToAction("UserLogin", "Login");
            }
            ViewBag.ErrorMessage = "Girdiğiniz kod hatalı, lütfen tekrar deneyin.";
            return View();
        }
    }
}
