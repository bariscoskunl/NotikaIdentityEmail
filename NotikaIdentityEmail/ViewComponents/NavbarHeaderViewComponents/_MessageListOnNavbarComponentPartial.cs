using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotikaIdentityEmail.Context;
using NotikaIdentityEmail.Entities;
using NotikaIdentityEmail.Models.MessageViewModels;

namespace NotikaIdentityEmail.ViewComponents.NavbarHeaderViewComponents
{
    public class _MessageListOnNavbarComponentPartial : ViewComponent
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly EmailContext _emailContext;

        public _MessageListOnNavbarComponentPartial(UserManager<AppUser> userManager, EmailContext emailContext)
        {
            _userManager = userManager;
            _emailContext = emailContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null)
            {
                return View(new List<MessageListWithUserInfoViewModel>());
            }
            var values = await _emailContext.Messages.Where(m => m.ReceiverEmail == user.Email && m.IsRead == false)
                .Select(m => new MessageListWithUserInfoViewModel
                {
                    FullName =  _emailContext.Users.Where(u => u.Email.ToLower() == m.SenderEmail.ToLower()).Select(u => u.Name + "" + u.Surname).FirstOrDefault(),
                    FullProfileImageUrl = _emailContext.Users.Where(u => u.Email.ToLower() == m.SenderEmail.ToLower()).Select(u => u.ImageUrl).FirstOrDefault(),
                    MessageDetail = m.MessageDetail,
                    SendDate = m.SendDate
                }).Take(5).ToListAsync();
            return View(values);
        }
    }
}
