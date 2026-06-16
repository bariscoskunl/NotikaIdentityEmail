using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NotikaIdentityEmail.Models.JwtModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NotikaIdentityEmail.Controllers
{
    public class TokenController : Controller
    {
        private readonly JwtSettingsModel _jwtSettingsModel;

        public TokenController(IOptions<JwtSettingsModel> jwtSettingsModel)
        {
            _jwtSettingsModel = jwtSettingsModel.Value;
        }

        public IActionResult Generate()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Generate(SimpleUserViewModel simpleUserViewModel)
        {
            var claim = new[]
            {
                new Claim ("name" , simpleUserViewModel.Name),
                new Claim ("surname" , simpleUserViewModel.Surname),
                new Claim ("city" , simpleUserViewModel.City),
                new Claim ("userName" , simpleUserViewModel.UserName),
                new Claim (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),// bu token için benzersiz bir id oluşturur
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettingsModel.Key)); // Burada appsettings.json dosyasındaki key'i kullanarak bir güvenlik anahtarı oluşturuyoruz
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); // Token'ı imzalamak için kullanılan kimlik bilgilerini oluşturuyoruz

            var token = new JwtSecurityToken(
                issuer: _jwtSettingsModel.Issuer, // Token'ı veren
                audience: _jwtSettingsModel.Audience,
                claims: claim, // Token'a eklenen claim'ler yani kullanıcı bilgileri
                expires: DateTime.UtcNow.AddMinutes(_jwtSettingsModel.ExpireMinutes), // Token'ın geçerlilik süresi
                signingCredentials: creds // Token'ı imzalamak için kullanılan kimlik bilgileri
                );

            simpleUserViewModel.Token = new JwtSecurityTokenHandler().WriteToken(token); // Token'ı string formatına çeviriyoruz ve modelin Token özelliğine atıyoruz
            return View(simpleUserViewModel); // Oluşturulan token'ı kullanıcıya gösteriyoruz
        }
    }
}
