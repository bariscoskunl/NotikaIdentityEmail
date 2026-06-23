using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotikaIdentityEmail.Context;
using NotikaIdentityEmail.Entities;

namespace NotikaIdentityEmail.ViewComponents
{
    public class _HeaderUserLayoutComponentPartial : ViewComponent
    {
        private EmailContext _emailContext;
        private UserManager<AppUser> _userManager;

        public _HeaderUserLayoutComponentPartial(EmailContext emailContext, UserManager<AppUser> userManager)
        {
            _emailContext = emailContext;
            _userManager = userManager;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                ViewBag.userEmailCount = 0;
                ViewBag.notificationCount = 0;
                return View();
            }

            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user != null)
            {
                var userEmail = user.Email;
                var userMessages = await _emailContext.Messages.Where(m => m.ReceiverEmail == userEmail).CountAsync();
                ViewBag.userEmailCount = userMessages;
            }
            else
            {
                ViewBag.userEmailCount = 0;
            }
            
            ViewBag.notificationCount = await _emailContext.Notifications.CountAsync();
            return View();
        }
    }
}
