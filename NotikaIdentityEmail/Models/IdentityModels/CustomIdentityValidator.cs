using Microsoft.AspNetCore.Identity;

namespace NotikaIdentityEmail.Models.IdentityModels
{
    public class CustomIdentityValidator : IdentityErrorDescriber //Her bir hata override edilecek.
    {
        public override IdentityError PasswordTooShort(int length)
        {
            return new IdentityError
            {
                Code = "PasswordTooShort",
                Description = $"Sifreniz en az {length} karakter icermelidir!"
            };
        }
        public override IdentityError PasswordRequiresLower()
        {
            return new IdentityError()
            {
                Code = "PasswordRequiresLower",
                Description = "Sifreniz en az 1 tene kucuk harf icermelidir!"
            };
        }
        public override IdentityError PasswordRequiresUpper()
        {
            return new IdentityError()
            {
                Code = "PasswordRequiresUpper",
                Description = "Sifreniz en az 1 tane buyuk harf icermelidir!"
            };
        }
        public override IdentityError PasswordRequiresDigit()
        {
            return new IdentityError()
            {
                Code = "PasswordRequiresDigit",
                Description = "Sifreniz en az 1 tene rakam icermelidir!"
            };
        }
        public override IdentityError PasswordRequiresNonAlphanumeric()
        {
            return new IdentityError()
            {
                Code = "PasswordRequiresNonAlphanumeric",
                Description = "Sifreniz en az 1 tene sembol icermelidir!"
            };
        }
        public override IdentityError DuplicateUserName(string userName)
        {
            return new IdentityError()
            {
                Code = "DuplicateUserName",
                Description = $"{userName} adli kullanici adi zaten alinmis, farkli bir kullanici adi deneyin!"
            };
        }
    }
}
