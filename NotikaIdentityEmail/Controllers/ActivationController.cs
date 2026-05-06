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
            var email = TempData["EmailMove"];
            TempData["Move"] = email;
            return View();
        }

        [HttpPost]
        public IActionResult UserActivation(int userCodeParamater)
        {
            if (TempData["Move"] == null)
            {
                return RedirectToAction("CreateUser", "Register");
            }

            string email = TempData["Move"].ToString();

            TempData.Keep("Move");

            var code = _context.Users.Where(x => x.Email == email).Select(y => y.ActivationCode).FirstOrDefault();

            if (userCodeParamater == code)
            {
                var value = _context.Users.Where(x => x.Email == email).FirstOrDefault();
                value.EmailConfirmed = true;
                _context.SaveChanges();
                return RedirectToAction("UserLogin", "Login");
            }
            ViewBag.ErrorMessage = "Girdiğiniz kod hatalı, lütfen tekrar deneyin.";
            return View();           
        }
    }
}
// yemi uukb anso zryu
