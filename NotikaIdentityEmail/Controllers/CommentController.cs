using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotikaIdentityEmail.Context;
using NotikaIdentityEmail.Entities;
using System.Text.Json;

namespace NotikaIdentityEmail.Controllers
{
    public class CommentController : Controller
    {
        private EmailContext _context;
        private UserManager<AppUser> _userManager;

        public CommentController(EmailContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult UserComments()
        {
            var values = _context.Comments.Include(c => c.AppUser).ToList();
            return View(values);
        }

        public IActionResult UserCommentList()
        {
            var values = _context.Comments.Include(c => c.AppUser).ToList();
            return View(values);
        }
        [HttpGet]
        public PartialViewResult CreateComment()
        {
            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment(Comment comment)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            comment.AppUserId = user.Id;
            comment.CommentDate = DateTime.Now;
            comment.CommentStatus = "Onay bekliyor";

            // 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
            //Toxic bert api analizi
            using (var client = new HttpClient())
            {
                var apiKey = "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";
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
                var requestBody = new // JSON formatında gönderilecek veriyi oluşturun             
                {
                    inputs = comment.CommentDetail,
                };
                var json = JsonSerializer.Serialize(requestBody); // olusturulan veriyi json formatına çeviriyoruz
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"); // donusumden sonra json formatında content oluşturuyoruz content icine atiyoruz

                try
                {
                    // api-inference adresi DNS sorunlarına yol açtığı için yeni yönlendirici (router) adresini kullanıyoruz
                    var response = await client.PostAsync("https://router.huggingface.co/hf-inference/models/unitary/toxic-bert", content);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync(); // gelen cevabı stringe çeviriyoruz
                        if (responseString.TrimStart().StartsWith("["))
                        {
                            var doc = JsonDocument.Parse(responseString);
                            bool isToxic = false;
                            foreach (var item in doc.RootElement[0].EnumerateArray())
                            {
                                string label = item.GetProperty("label").GetString();
                                double score = item.GetProperty("score").GetDouble();

                                if (score > 0.5)
                                {
                                    isToxic = true;
                                    break;
                                }
                            }
                            comment.CommentStatus = isToxic ? "Toksik yorum" : "Yorum onaylandı";
                        }
                        else
                        {
                            comment.CommentStatus = "Onay Bekliyor";
                        }
                    }
                    // API başarısız yanıt dönerse, statü zaten "Onay bekliyor" olarak kalır
                    else
                    {
                        var errorBody = await response.Content.ReadAsStringAsync();
                        TempData["ApiDebug"] = $"API Hata Kodu: {(int)response.StatusCode} - Yanıt: {errorBody}";
                    }
                }
                catch (Exception ex)
                {
                    // Bağlantı hatası durumunda, statü zaten "Onay bekliyor" olarak kalır
                    TempData["ApiDebug"] = $"Bağlantı Hatası: {ex.Message}";
                }

            }

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("UserCommentList");
        }
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            _context.Remove(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("UserCommentList");
        }
    }
}
