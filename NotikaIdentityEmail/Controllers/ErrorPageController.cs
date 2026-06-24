using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NotikaIdentityEmail.Controllers
{
    [AllowAnonymous]
    public class ErrorPageController : Controller
    {
        // Belirli hata kodları için doğrudan erişilebilir rotaların tanımlanması
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

        // Program.cs UseStatusCodePagesWithReExecute üzerinden gelen hataların yakalanıp, URL bozulmadan ilgili görünüme (View) aktarılması
        [Route("Error/{statusCode}")]
        public IActionResult HandleError(int statusCode)
        {
            if (statusCode == 404)
            {
                return View("Page404");
            }
            if (statusCode == 401)
            {
                return View("Page401");
            }
            if (statusCode == 403)
            {
                return View("Page403");
            }

            // Tanımlanmamış diğer tüm hatalar için varsayılan görünüm
            return View("Page404");
        }
    }
}