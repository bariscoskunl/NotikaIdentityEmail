# Notika Identity & Email Management System

Bu proje, ASP.NET Core kullanılarak geliştirilmiş, **Notika** arayüz temasına entegre edilmiş kapsamlı bir Kimlik Doğrulama (Identity) ve E-posta (Email) yönetim sistemidir. 

Projenin temel amacı, güvenli kullanıcı kimlik doğrulama işlemleri (Login, Register, Password Reset vb.) sağlamak ve sistem içerisinde dahili bir mesajlaşma/e-posta altyapısı sunmaktır. Aynı zamanda modern bir kullanıcı arayüzü ile kullanıcı deneyimini (UX) en üst seviyeye çıkarmayı hedefler.

---

## 🏗️ Projenin Mimari Yapısı

Proje, genel hatlarıyla **Monolitik (Monolithic)** bir yapı üzerine inşa edilmiş olup, **MVC (Model-View-Controller)** tasarım desenini benimsemektedir. Sürdürülebilirlik ve temiz kod yazımı (Clean Code) prensiplerine uygun olarak farklı katmanlar ve servisler kullanılmıştır.

### 1. MVC (Model-View-Controller) Deseni
- **Models:** Veritabanı tablolarını temsil eden Entity sınıfları (Örn: `AppUser`, `AppRole`, `Message`) ve arayüze veri taşımak için kullanılan ViewModel sınıfları (Örn: `UserLoginViewModel`, `UserEditViewModel`) bu katmanda yer alır.
- **Views:** Kullanıcı arayüzünü (UI) oluşturan Razor View (`.cshtml`) dosyalarıdır. "Notika" temasının bileşenleri buradaki `_UserLayout`, `_LoginLayout` gibi şablonlarla yönetilir.
- **Controllers:** İstemciden (Browser) gelen istekleri (HTTP GET/POST) yakalayan, gerekli servisleri çağırarak iş mantığını yürüten ve sonucu View katmanına ileten kontrolcülerdir (Örn: `LoginController`, `MessageController`).

### 2. Veri Erişim Katmanı (Data Access)
Veritabanı işlemleri için **Entity Framework Core** ve **Code-First** yaklaşımı tercih edilmiştir. 
Uygulama, veritabanı şemasını (Tablolar, İlişkiler) sınıflar üzerinden otomatik olarak SQL Server üzerinde inşa eder.

### 3. Kimlik Doğrulama Katmanı (Identity & Security)
- **ASP.NET Core Identity:** Kullanıcı yönetimi, parola hash'leme (şifreleme), rol atamaları ve oturum (Cookie/Session) yönetimi bu kütüphane üzerinden sağlanır.
- **JWT (JSON Web Token):** Sadece tarayıcı üzerinden değil, dış servisler veya API tabanlı istekler için güvenli veri iletişimi sağlamak adına projeye JWT Token mekanizması entegre edilmiştir.
- **OAuth2 (Google Login):** Kullanıcıların karmaşık kayıt aşamalarını atlayıp Google hesaplarıyla tek tıkla uygulamaya giriş yapabilmesini sağlayan harici giriş (External Login) servisi.

### 4. Servisler & Bağımlılık Enjeksiyonu (Dependency Injection)
Projedeki e-posta gönderme (SMTP), token üretme veya özel işlemler servis sınıflarına ayrılarak (Örn: `IEmailService`) gevşek bağlı (loosely-coupled) bir mimari elde edilmiştir. ASP.NET Core'un kendi `IoC (Inversion of Control)` container'ı kullanılarak bu bağımlılıklar sisteme enjekte edilmektedir.

### 5. Temiz Kod (Clean Code) ve Profesyonel Mimari
- **Extension Metotları ile Program.cs Yönetimi:** Projenin ana yapılandırma dosyası olan `Program.cs`'in şişmesini önlemek amacıyla JWT ayarları, veritabanı bağlantıları ve Identity konfigürasyonları özel **Extension** sınıflarına (`AuthenticationExtensions`, `DatabaseAndIdentityExtensions` vb.) taşınarak modüler bir yapı elde edilmiştir.
- **Teknik Dokümantasyon Standardı:** Proje genelindeki kritik algoritmalar (Örn: Dış sağlayıcı ile Login mekanizması, Fluent API kuralları, Token üretimi), sadece kodun ne yaptığını değil "neden" o şekilde yazıldığını da açıklayan kısa ve profesyonel yorum bloklarıyla zenginleştirilmiştir.

---

## 🚀 Temel Özellikler

- **Gelişmiş Profil Yönetimi:** Kullanıcılar kendilerine ait resim, adres, e-posta gibi verileri modern bir arayüzden güncelleyebilir.
- **Şifre Sıfırlama & Mail Onayı:** Kullanıcılar şifrelerini unuttuğunda sisteme kayıtlı e-posta adreslerine "Şifre Sıfırlama Linki" gönderilir (MailKit kullanılarak).
- **İç Mesajlaşma Sistemi (Inbox/Sent):** Kullanıcıların birbirlerine platform üzerinden doğrudan mesaj atabileceği, mesajlarını okuyup silebileceği gelişmiş posta kutusu altyapısı.
- **Yapay Zeka Entegrasyonu:** Hugging Face API anahtarı kullanılarak sisteme opsiyonel AI yetenekleri eklenebilecek altyapı hazırlanmıştır.

---

## ⚙️ Kurulum & Yapılandırma

Bu projeyi kendi ortamınızda (Localhost) çalıştırmak için aşağıdaki adımları izlemelisiniz:

### 1. `appsettings.json` Dosyasının Ayarlanması
Güvenlik prensipleri (Best Practices) gereği projede veritabanı bağlantı şifreleri, API keyler ve Email şifrelerinin bulunduğu asıl `appsettings.json` dosyası GitHub'a yüklenmez (`.gitignore` ile gizlenmiştir). 

Projeyi bilgisayarınıza klonladıktan sonra, projenin kök dizininde bulunan **`appsettings.Template.json`** dosyasının adını **`appsettings.json`** olarak değiştirin ve içindeki alanları kendi yerel bilgilerinize göre doldurun:

```json
{
  "ConnectionStrings": {
    "Default": "Server=BILGISAYAR_ADINIZ;Database=NotikaEmailDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "Key": "GIZLI_VE_UZUN_BIR_ANAHTAR_YAZIN"
  },
  "EmailSettings": {
    "SenderEmail": "ornek_mail@gmail.com",
    "Password": "google_uygulama_sifresi" 
    // Not: Normal gmail şifrenizi değil, Google hesabınızdan aldığınız 16 haneli 'Uygulama Şifresi'ni kullanmalısınız.
  },
  "GoogleLogin": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID",
    "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
  }
}
```

### 2. Veritabanının Oluşturulması (Entity Framework Migration)
Gerekli paketlerin inmesini sağladıktan sonra, Visual Studio'da **Package Manager Console (PMC)** penceresini açın (veya Terminal'de `dotnet ef database update` yazın) ve aşağıdaki komutu çalıştırarak veritabanınızı oluşturun:

```bash
Update-Database
```
*(Eğer daha önceden hiçbir Migration alınmamışsa, önce `Add-Migration InitialCreate` komutunu çalıştırmanız gerekebilir).*

### 3. Projeyi Çalıştırma (Run)
Veritabanınız SQL Server üzerinde başarıyla oluşturulduktan sonra projeyi başlatabilirsiniz. Proje sizi doğrudan Notika temasına giydirilmiş Giriş Yap (Login) ekranına yönlendirecektir. Henüz hesabınız olmadığı için "Kayıt Ol" seçeneğine tıklayarak ilk kullanıcınızı oluşturabilir ve sistemi test etmeye başlayabilirsiniz.

---

## 🎨 UI/UX Tasarım ve Tema Detayları

Proje **Notika Admin Template** üzerine inşa edilerek görsel olarak zenginleştirilmiştir:
- **Ortak Layout (Şablon) Yönetimi:** Kod tekrarını (Don't Repeat Yourself) önlemek için kimlik doğrulama sayfalarına (Login, Register vb.) özel `_LoginLayout`, ana panel sayfalarına ise `_UserLayout` atanmıştır.
- **Profil Kartı Mimarisi:** Giriş yapan kullanıcılar, modern ve iki kolonlu ferah bir "Edit Profile" ekranı ile karşılaşır. Sol kısımda detaylı bir profil kartı varken sağ kısımda şık bir form bulunur.
- **Akıllı Yönlendirmeler:** Zaten giriş yapmış (Authenticated) bir kullanıcı `/Login` veya `/Register` sayfasına girmeye çalışırsa sistem onu yakalar ve otomatik olarak ana sayfaya (Dashboard) yönlendirir.
- **Kullanıcı Dostu Navigasyon (Breadcrumb):** Sistemde gezinirken, örneğin Mesajlara hızlıca ulaşabilmeniz için Breadcrumb alanında statikleştirilmiş özel "Yeni Mesaj Oluştur (Compose)" kısayolu bulunur.

---

## 📄 Lisans

Bu proje **MIT Lisansı** ile açık kaynaklı olarak paylaşılmaktadır. Özgürce kullanabilir, kopyalayabilir, değiştirebilir ve ticari projelerinizde yer verebilirsiniz. Daha fazla hukuki detay için projedeki [LICENSE](LICENSE) dosyasına göz atabilirsiniz.
