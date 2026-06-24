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
            // JWT token için gerekli claim (kullanıcı bilgileri) listesi
            var claim = new[]
            {
                new Claim ("name" , simpleUserViewModel.Name),
                new Claim ("surname" , simpleUserViewModel.Surname),
                new Claim ("city" , simpleUserViewModel.City),
                new Claim ("userName" , simpleUserViewModel.UserName),
                new Claim (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Güvenlik anahtarı ve imzalama kimlik bilgileri
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettingsModel.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Token oluşturma ve temel konfigürasyonu
            var token = new JwtSecurityToken(
                issuer: _jwtSettingsModel.Issuer, // Token'ı oluşturan ve yayınlayan taraf (örn: Auth sunucumuz)
                audience: _jwtSettingsModel.Audience, // Bu token'ı tüketmeye yetkili olan istemci veya hedef API
                claims: claim, // Token içerisine gömülen şifrelenmiş kullanıcı verileri (Payload)
                expires: DateTime.UtcNow.AddMinutes(_jwtSettingsModel.ExpireMinutes), // Güvenlik gereği token'ın ne kadar süre geçerli kalacağı
                signingCredentials: creds // Token'ın değiştirilmediğini kanıtlayan HMAC SHA256 dijital imzası
            );

            // Oluşturulan token'ın string formata çevrilerek modele atanması
            simpleUserViewModel.Token = new JwtSecurityTokenHandler().WriteToken(token);
            return View(simpleUserViewModel);
        }
    }
}
