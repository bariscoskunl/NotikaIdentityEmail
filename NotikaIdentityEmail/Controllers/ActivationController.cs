using Microsoft.AspNetCore.Mvc;

namespace NotikaIdentityEmail.Controllers
{
    public class ActivationController : Controller
    {


        [HttpGet]
        public IActionResult UserActivation()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UserActivation(string x)
        {
            return RedirectToAction("UserLogin", "Login");
        }
    }
}
// yemi uukb anso zryu
