using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotikaIdentityEmail.Context;

namespace NotikaIdentityEmail.ViewComponents.NavbarHeaderViewComponents
{
    public class _NotificationListOnNavbarComponentPartial : ViewComponent
    {
        private readonly EmailContext _context;

        public _NotificationListOnNavbarComponentPartial(EmailContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var values = await _context.Notifications.OrderByDescending(n => n.NotificationId).Take(5).ToListAsync();
            return View(values);
        }
    }
}
