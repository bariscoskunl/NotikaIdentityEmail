using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NotikaIdentityEmail.Controllers
{
    [AllowAnonymous]
    public class ErrorPageController : Controller
    {
        // Doğrudan bu rotalara istek atılırsa da ilgili sayfalar açılır
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

        // Program.cs'den gelen StatusCode'u yakalayan ana metot
        [Route("Error/{statusCode}")]
        public IActionResult HandleError(int statusCode)
        {
            // RedirectToAction yerine direkt ilgili View ismini çağırıyoruz.
            // Böylece URL bozulmaz ve butonlar kararlı çalışır.
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

            // Diğer beklenmeyen tüm hatalar için genel bir hata görünümü
            return View("Page404");
        }
    }
}