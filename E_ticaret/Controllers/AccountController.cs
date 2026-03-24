using E_ticaret.Modellerim;
using E_ticaret.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace E_ticaret.Controllers
{
    public class AccountController : Controller
    {
        private readonly EticaretContext _context;
        public AccountController(EticaretContext context) { _context = context; }

        // --- ŞİFRE HASHLEME (Güvenlik İçin) ---
        private string SifreHashle(string sifre)
        {
            if (string.IsNullOrEmpty(sifre)) return "";
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(sifre);
            var hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        // --- GİRİŞ İŞLEMLERİ ---
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string email, string sifre)
        {
            string hashed = SifreHashle(sifre);
            var user = _context.Kullanicilars.FirstOrDefault(x => x.Email == email && x.Sifre == hashed);
            if (user != null)
            {
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserName", user.AdSoyad ?? "Kullanıcı");
                HttpContext.Session.SetString("UserEmail", user.Email ?? "");
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = "E-posta veya şifre hatalı!";
            return View();
        }

        public IActionResult Logout() { HttpContext.Session.Clear(); return RedirectToAction("Index", "Home"); }

        // --- KAYIT VE DOĞRULAMA ---
        [HttpGet] public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(Kullanicilar model, string SifreTekrar)
        {
            if (model.Sifre != SifreTekrar) { ViewBag.Error = "Şifreler uyuşmuyor!"; return View(); }
            if (_context.Kullanicilars.Any(x => x.Email == model.Email)) { ViewBag.Error = "Bu e-posta zaten kayıtlı!"; return View(); }

            string onayKodu = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("TempUser_AdSoyad", model.AdSoyad ?? "");
            HttpContext.Session.SetString("TempUser_Email", model.Email ?? "");
            HttpContext.Session.SetString("TempUser_Sifre", SifreHashle(model.Sifre ?? ""));
            HttpContext.Session.SetString("TempUser_Kod", onayKodu);

            // TODO: MailServis.Gonder(model.Email, onayKodu);

            return RedirectToAction("Verify");
        }

        [HttpGet] public IActionResult Verify() { ViewBag.Email = HttpContext.Session.GetString("TempUser_Email"); return View(); }

        [HttpPost]
        public IActionResult Verify(string kod)
        {
            if (HttpContext.Session.GetString("TempUser_Kod") == kod)
            {
                var yeniUser = new Kullanicilar
                {
                    AdSoyad = HttpContext.Session.GetString("TempUser_AdSoyad"),
                    Email = HttpContext.Session.GetString("TempUser_Email"),
                    Sifre = HttpContext.Session.GetString("TempUser_Sifre"),
                    KayitTarihi = DateTime.Now,
                    Durum = true
                };
                _context.Kullanicilars.Add(yeniUser);
                _context.SaveChanges();
                HttpContext.Session.Remove("TempUser_Kod");
                return RedirectToAction("Login");
            }
            ViewBag.Error = "Kod hatalı!"; return View();
        }

        // --- PROFİL VE GÜNCELLEME ---
        [HttpGet]
        public IActionResult Profile()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login");

            var user = _context.Kullanicilars.Find(int.Parse(userIdStr));
            return View(user);
        }

        [HttpPost]
        public IActionResult UpdateProfile(Kullanicilar model, string? YeniSifre)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login");

            var user = _context.Kullanicilars.Find(int.Parse(userIdStr));
            if (user != null)
            {
                user.AdSoyad = model.AdSoyad;
                user.Telefon = model.Telefon;

                if (!string.IsNullOrEmpty(YeniSifre))
                {
                    user.Sifre = SifreHashle(YeniSifre);
                }

                _context.SaveChanges();
                HttpContext.Session.SetString("UserName", user.AdSoyad);
                TempData["Message"] = "Bilgileriniz başarıyla güncellendi.";
            }
            return RedirectToAction("Profile");
        }

        // --- SİPARİŞLERİM ---
        public IActionResult Orders()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login");
            int userId = int.Parse(userIdStr);

            var siparisler = _context.Siparislers
                .Where(x => x.KullaniciId == userId && x.Durum != "Sepette")
                .OrderByDescending(x => x.SiparisTarihi)
                .ToList();

            ViewBag.Urunler = _context.Urunlers.ToList();
            return View(siparisler);
        }

        // --- İADE VE İPTAL SÜREÇLERİ ---
        [HttpPost]
        public IActionResult IadeKaydet(int siparisId, string neden)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login");

            var siparis = _context.Siparislers.Find(siparisId);
            if (siparis != null && siparis.KullaniciId == int.Parse(userIdStr))
            {
                siparis.Durum = "İade Talebi Oluşturuldu";
                siparis.IadeNedeni = neden;
                siparis.TalepTarihi = DateTime.Now;
                _context.SaveChanges();
                TempData["Message"] = "İade talebiniz işleme alınmıştır.";
            }
            return RedirectToAction("Orders");
        }

        [HttpPost]
        public IActionResult SiparisIptal(int id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login");

            var siparis = _context.Siparislers.Find(id);
            if (siparis != null && siparis.KullaniciId == int.Parse(userIdStr))
            {
                if (siparis.Durum == "Sipariş Alındı" || siparis.Durum == "Hazırlanıyor")
                {
                    var tumPaket = _context.Siparislers.Where(x => x.SiparisNo == siparis.SiparisNo).ToList();
                    foreach (var item in tumPaket)
                    {
                        var urun = _context.Urunlers.Find(item.UrunId);
                        if (urun != null) urun.Adet += item.Adet;
                        item.Durum = "İptal Edildi";
                    }
                    _context.SaveChanges();
                    TempData["Message"] = "Siparişiniz iptal edildi ve stoklar güncellendi.";
                }
            }
            return RedirectToAction("Orders");
        }

        // --- ADRES YÖNETİMİ ---
        [HttpGet]
        public IActionResult Adreslerim()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login");

            var adresler = _context.KullaniciAdresleris
                .Where(x => x.KullaniciId == int.Parse(userIdStr))
                .ToList();
            return View(adresler);
        }

        // 1. Sayfayı açan kısım (Eksikse 405 veya 404 verebilir)
        [HttpGet]
        public IActionResult AdresEkle()
        {
            return View();
        }

        // 2. Kaydı yapan kısım (Senin metodun)
        [HttpPost]
        public IActionResult AdresEkle(KullaniciAdresleri model)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userIdStr))
            {
                model.KullaniciId = int.Parse(userIdStr);
                // Eğer veritabanında bu alanlar boş geçilemezse hata almamak için:
                // model.KayitTarihi = DateTime.Now; 

                _context.KullaniciAdresleris.Add(model);
                _context.SaveChanges();
                TempData["Message"] = "Yeni adres kaydedildi.";

                // Eğer profil sayfanın adı Index ise ("Index", "Account") yapmalısın.
                return RedirectToAction("Profile");
            }
            return RedirectToAction("Login");
        }

        public IActionResult AdresSil(int id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login");

            var adres = _context.KullaniciAdresleris.Find(id);
            if (adres != null && adres.KullaniciId == int.Parse(userIdStr))
            {
                _context.KullaniciAdresleris.Remove(adres);
                _context.SaveChanges();
                TempData["Message"] = "Adres başarıyla silindi.";
            }
            return RedirectToAction("Profile");
        }

        // --- ŞİFREMİ UNUTTUM SÜREÇLERİ (GÜNCEL & GERÇEK MAİL GÖNDEREN) ---

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            var user = _context.Kullanicilars.FirstOrDefault(x => x.Email == email);
            if (user == null)
            {
                ViewBag.Error = "Bu e-posta adresi sistemde kayıtlı değil.";
                return View();
            }

            // 1. Token Oluştur
            string token = Guid.NewGuid().ToString();
            user.PasswordResetToken = token;
            user.ResetTokenExpires = DateTime.Now.AddHours(1);
            _context.SaveChanges();

            // 2. Sıfırlama Linkini Hazırla
            var resetLink = Url.Action("ResetPassword", "Account", new { token = token }, Request.Scheme);

            // 3. GERÇEK MAİL GÖNDERİMİ (Home'daki çalışan ayarlarınla)
            try
            {
                using (var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    Credentials = new NetworkCredential("beyzakblt@gmail.com", "eqvm lqjb ubnm hdok") // Senin çalışan şifren
                })
                {
                    var mailMessage = new MailMessage("beyzakblt@gmail.com", email)
                    {
                        Subject = "Şifre Sıfırlama Talebi - Beyza E-Ticaret",
                        Body = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                        <h2 style='color: #784BA0;'>Şifre Sıfırlama Talebi</h2>
                        <p>Merhaba,</p>
                        <p>Hesabınızın şifresini sıfırlamak için bir talepte bulundunuz. Aşağıdaki butona tıklayarak yeni şifrenizi belirleyebilirsiniz:</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{resetLink}' style='background: linear-gradient(135deg, #FF3CAC, #784BA0); color: white; padding: 12px 25px; text-decoration: none; border-radius: 50px; font-weight: bold;'>Şifremi Sıfırla</a>
                        </div>
                        <p style='color: #777; font-size: 12px;'>Eğer bu talebi siz yapmadıysanız bu maili dikkate almayınız. Link 1 saat geçerlidir.</p>
                    </div>",
                        IsBodyHtml = true
                    };
                    smtp.Send(mailMessage);
                }
                TempData["Message"] = "Şifre sıfırlama linki e-postanıza ( " + email + " ) başarıyla gönderildi!";
            }
            catch (Exception ex)
            {
                // Eğer mail gitmezse hatayı ekranda gör ama token kaydolmuş olur
                TempData["Message"] = "Token oluşturuldu fakat mail gönderilemedi: " + ex.Message;
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            // Linke tıklandığında token geçerli mi ve süresi dolmamış mı kontrol et
            var user = _context.Kullanicilars.FirstOrDefault(x => x.PasswordResetToken == token && x.ResetTokenExpires > DateTime.Now);

            if (user == null)
            {
                return Content("Geçersiz veya süresi dolmuş şifre sıfırlama linki!");
            }

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string token, string newPassword)
        {
            var user = _context.Kullanicilars.FirstOrDefault(x => x.PasswordResetToken == token && x.ResetTokenExpires > DateTime.Now);

            if (user != null)
            {
                // Yeni şifreyi hashleyerek kaydet
                user.Sifre = SifreHashle(newPassword);
                user.PasswordResetToken = null; // Güvenlik için token'ı imha et
                user.ResetTokenExpires = null;
                _context.SaveChanges();

                TempData["Message"] = "Şifreniz başarıyla güncellendi. Giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }

            ViewBag.Error = "Bir hata oluştu veya linkin süresi dolmuş.";
            return View();
        }
    }
}