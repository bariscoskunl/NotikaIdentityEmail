using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotikaIdentityEmail.Context;
using NotikaIdentityEmail.Entities;
using System.IdentityModel.Tokens.Jwt;

namespace NotikaIdentityEmail.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly EmailContext _emailContext;

        public CategoryController(EmailContext emailContext)
        {
            _emailContext = emailContext;
        }
        public IActionResult CategoryList()
        {
            var token = Request.Cookies["jwtToken"];
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Lütfen önce giriş yapın.";
                return RedirectToAction("UserLogin", "Login");
            }

            JwtSecurityToken jwt;
            try
            {
                var handler = new JwtSecurityTokenHandler();
                jwt = handler.ReadJwtToken(token);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Geçersiz bir token sağlandı.";
                return RedirectToAction("UserLogin", "Login");
            }

            var city = jwt.Claims.FirstOrDefault(c => c.Type == "city")?.Value;
            if (city != "Antalya")
            {
                return Forbid(); // Kullanıcı Antalya şehrinde değilse erişimi engelle
            }


            var values = _emailContext.Categories.ToList();
            return View(values);
        }
        [HttpGet]
        public IActionResult AddCategory()
        {
            return View();
        }
        [HttpPost]
        public IActionResult AddCategory(Category category)
        {
            category.CategoryStatus = true; // Kategori durumu varsayılan olarak aktif olacak şekilde ayarlanıyor
            _emailContext.Categories.Add(category);
            _emailContext.SaveChanges();
            return RedirectToAction("CategoryList");
        }
        [HttpGet]
        public IActionResult UpdateCategory(int id)
        {
            var category = _emailContext.Categories.FirstOrDefault(c => c.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        [HttpPost]
        public IActionResult UpdateCategory(Category category)
        {
           var existCategory = _emailContext.Categories.FirstOrDefault(c => c.CategoryId == category.CategoryId);
            if (existCategory != null)
            {
               existCategory.CategoryName = category.CategoryName;
               existCategory.CategoryIconUrl = category.CategoryIconUrl;
                _emailContext.SaveChanges();
                return RedirectToAction("CategoryList");
            }
            return NotFound();
        }

        public IActionResult DeleteCategory(int id)
        {
            var category = _emailContext.Categories.FirstOrDefault(c => c.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }
            _emailContext.Categories.Remove(category);
            _emailContext.SaveChanges();
            return RedirectToAction("CategoryList");
        }

    }
}
