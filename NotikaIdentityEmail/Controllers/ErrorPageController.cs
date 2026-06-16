using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NotikaIdentityEmail.Controllers
{
    [AllowAnonymous]
    public class ErrorPageController : Controller
    {
        [Route("Error/404")]
        public IActionResult Page404()
        {
            return View();
        }

        [Route("Error/401")]
        public IActionResult Page401()
        {
            return View();
        }

        [Route("Error/403")]
        public IActionResult Page403()
        {
            return View();
        }

        [Route("Error/{statusCode}")]
        public IActionResult HandleError(int statusCode) {
             
            if (statusCode == 404)
            {
                return RedirectToAction("Page404");
            }
            if (statusCode == 401)
            {
                return RedirectToAction("Page401");
            }
            if (statusCode == 403)
            {
                return View("Page403");
            }
            return View(statusCode);
        }
    }
}
