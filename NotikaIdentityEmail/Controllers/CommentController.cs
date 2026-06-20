using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotikaIdentityEmail.Context;
using NotikaIdentityEmail.Entities;

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

            comment.CommentStatus = "Onay Bekliyor";
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("UserCommentList");
        }
        public async Task<IActionResult> DeleteComment(int id)
        { 
            var comment =await _context.Comments.FindAsync(id);
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
