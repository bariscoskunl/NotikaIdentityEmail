using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotikaIdentityEmail.Context;
using NotikaIdentityEmail.Entities;
using System.Text;
using System.Text.Json;

namespace NotikaIdentityEmail.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private EmailContext _context;
        private UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;

        public CommentController(EmailContext context, UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }
        public IActionResult UserComments()
        {
            var values = _context.Comments.Include(c => c.AppUser).Where(c => c.CommentStatus == "Yorum onaylandı").ToList();
            return View(values);
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult UserCommentList()
        {
            var values = _context.Comments.Include(c => c.AppUser).ToList();
            return View(values);
        }
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Member,User")]
        public PartialViewResult CreateComment()
        {
            return PartialView();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Member,User")]
        public async Task<IActionResult> CreateComment(Comment comment)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            comment.AppUserId = user.Id;
            comment.CommentDate = DateTime.Now;
            comment.CommentStatus = "Onay bekliyor";

            // Yorum içeriğinin HuggingFace Toxic-BERT modeli ile yapay zeka tarafından analiz edilmesi
            using (var client = new HttpClient())
            {
                var apiKey = _configuration["HuggingFace:apiKey"];
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                try
                {
                    var translateRequestBody = new
                    {
                        inputs = comment.CommentDetail,
                    };
                    var translateJson = JsonSerializer.Serialize(translateRequestBody);
                    var translateContent = new StringContent(translateJson, Encoding.UTF8, "application/json");

                    var translateResponse = await client.PostAsync("https://router.huggingface.co/hf-inference/models/Helsinki-NLP/opus-mt-tr-en", translateContent);
                    var translateResponseString = await translateResponse.Content.ReadAsStringAsync();

                    // Model İngilizce çalıştığı için gelen metni çevirerek işleme alıyoruz, hata olursa orijinal metne dönülüyor
                    string englishText = comment.CommentDetail;
                    if (translateResponseString.TrimStart().StartsWith("["))
                    {
                        var translateDoc = JsonDocument.Parse(translateResponseString);
                        englishText = translateDoc.RootElement[0].GetProperty("translation_text").GetString();
                    }

                    var toxicRequestBody = new
                    {
                        inputs = englishText,
                    };



                    // JsonSerializer: C# nesnesini (toxicRequestBody) API'nin anlayabileceği JSON metnine çevirir
                    var toxicJson = JsonSerializer.Serialize(toxicRequestBody);
                    // StringContent: JSON metnini HTTP POST isteğinin gövdesine (body) UTF-8 formatında yerleştirmek için sarmalar
                    var toxicContent = new StringContent(toxicJson, System.Text.Encoding.UTF8, "application/json");

                    // DNS sorunlarını önlemek için yönlendirici (router) üzerinden Toxic-BERT modeline istek atılması
                    var toxicResponse = await client.PostAsync("https://router.huggingface.co/hf-inference/models/unitary/toxic-bert", toxicContent);
                    if (toxicResponse.IsSuccessStatusCode)
                    {
                        var toxicResponseString = await toxicResponse.Content.ReadAsStringAsync();
                        if (toxicResponseString.TrimStart().StartsWith("["))
                        {
                            var toxicDoc = JsonDocument.Parse(toxicResponseString);
                            bool isToxic = false;
                            foreach (var item in toxicDoc.RootElement[0].EnumerateArray())
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
                    // API başarısız yanıt dönerse veya bağlantı hatası yaşanırsa, yorum güvenli bir şekilde "Onay bekliyor" statüsünde bırakılır
                    else
                    {
                        var errorBody = await toxicResponse.Content.ReadAsStringAsync();
                        TempData["ApiDebug"] = $"API Hata Kodu: {(int)toxicResponse.StatusCode} - Yanıt: {errorBody}";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ApiDebug"] = $"Bağlantı Hatası: {ex.Message}";
                }

            }

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("UserCommentList");
        }
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("UserCommentList");
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult CommentStatusChangeToToxic(int id)
        {
            var values = _context.Comments.Find(id);
            values.CommentStatus = "Toksik yorum";
            _context.SaveChanges();
            return RedirectToAction("UserCommentList");
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult CommentStatusChangeToPasive(int id)
        {
            var values = _context.Comments.Find(id);
            values.CommentStatus = "Pasif yorum";
            _context.SaveChanges();
            return RedirectToAction("UserCommentList");
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult CommentStatusChangeToActive(int id)
        {
            var values = _context.Comments.Find(id);
            values.CommentStatus = "Yorum onaylandı";
            _context.SaveChanges();
            return RedirectToAction("UserCommentList");
        }
    }
}
