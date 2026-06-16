using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotikaIdentityEmail.Entities;
using NotikaIdentityEmail.Models.IdentityModels;

namespace NotikaIdentityEmail.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public ProfileController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<IActionResult> EditProfile()
        {
            var values = await _userManager.FindByNameAsync(User.Identity.Name);
            UserEditViewModel userEditViewModel = new UserEditViewModel();
            userEditViewModel.Name = values.Name;
            userEditViewModel.Surname = values.Surname;
            userEditViewModel.Email = values.Email;
            userEditViewModel.UserName = values.UserName;
            userEditViewModel.Password = values.PasswordHash;
            userEditViewModel.PhoneNumber = values.PhoneNumber;
            userEditViewModel.ImageUrl = values.ImageUrl;
            userEditViewModel.City = values.City;

            return View(userEditViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(UserEditViewModel model)
        {
            if (model.Password == model.PasswordConfirm)
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                user.Name = model.Name;
                user.Surname = model.Surname;
                user.UserName = model.UserName;
                user.PhoneNumber = model.PhoneNumber;
                user.City = model.City;
                user.Email = model.Email;
                user.ImageUrl = model.ImageUrl;
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, model.Password);
                await _userManager.UpdateAsync(user);
            }
            return View();
        }
    }
}
