using Microsoft.AspNetCore.Mvc;
using E_ticaret.Models;
using E_ticaret.Modellerim;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json; // Bilgileri session'da tutmak için gerekebilir

namespace E_ticaret.Controllers
{
    public class AccountController : Controller
    {
        private readonly EticaretContext _context;
        public AccountController(EticaretContext context) { _context = context; }

        [HttpGet] public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string email, string sifre)
        {
            string hashed = SifreHashle(sifre);
            var user = _context.Kullanicilars.FirstOrDefault(x => x.Email == email && x.Sifre == hashed);
            if (user != null)
            {
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserName", user.AdSoyad);
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = "E-posta veya şifre hatalı!";
            return View();
        }

        [HttpGet] public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(Kullanicilar model, string SifreTekrar)
        {
            if (model.Sifre != SifreTekrar) { ViewBag.Error = "Şifreler uyuşmuyor!"; return View(); }
            if (_context.Kullanicilars.Any(x => x.Email == model.Email)) { ViewBag.Error = "Bu e-posta zaten kayıtlı!"; return View(); }

            // 1. Onay Kodu Üret
            string onayKodu = new Random().Next(100000, 999999).ToString();

            // 2. Kullanıcı Bilgilerini Geçici Olarak Session'a At
            HttpContext.Session.SetString("TempUser_AdSoyad", model.AdSoyad);
            HttpContext.Session.SetString("TempUser_Email", model.Email);
            HttpContext.Session.SetString("TempUser_Sifre", SifreHashle(model.Sifre));
            HttpContext.Session.SetString("TempUser_Kod", onayKodu);

            // 3. Veritabanına kaydetmeden önce Mail Gönder
            try
            {
                MailHelper.SendCode(model.Email, onayKodu);
                return RedirectToAction("Verify");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Mail gönderim hatası: " + ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult Verify()
        {
            ViewBag.Email = HttpContext.Session.GetString("TempUser_Email");
            return View();
        }

        [HttpPost]
        public IActionResult Verify(string kod)
        {
            string sessionKod = HttpContext.Session.GetString("TempUser_Kod");

            if (sessionKod == kod)
            {
                // KOD DOĞRU! Şimdi veritabanına gerçekten kaydediyoruz.
                Kullanicilar yeniUser = new Kullanicilar
                {
                    AdSoyad = HttpContext.Session.GetString("TempUser_AdSoyad"),
                    Email = HttpContext.Session.GetString("TempUser_Email"),
                    Sifre = HttpContext.Session.GetString("TempUser_Sifre"),
                    KayitTarihi = DateTime.Now,
                    Durum = true // Artık onaylı
                };

                _context.Kullanicilars.Add(yeniUser);
                _context.SaveChanges();

                // Session'ı temizle
                HttpContext.Session.Remove("TempUser_Kod");

                return RedirectToAction("Login");
            }

            ViewBag.Error = "Onay kodu hatalı!";
            ViewBag.Email = HttpContext.Session.GetString("TempUser_Email");
            return View();
        }

        public IActionResult Logout() { HttpContext.Session.Clear(); return RedirectToAction("Index", "Home"); }

        private string SifreHashle(string sifre)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(sifre);
            var hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
        // AccountController.cs içine ekle

        [HttpGet]
        public IActionResult Profile()
        {
            // Session'dan giriş yapan kullanıcının ID'sini alıyoruz
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login");

            int userId = int.Parse(userIdStr);
            var kullanici = _context.Kullanicilars.Find(userId);

            return View(kullanici);
        }

        [HttpPost]
        public IActionResult UpdateProfile(Kullanicilar model)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login");

            int userId = int.Parse(userIdStr);
            var kullanici = _context.Kullanicilars.Find(userId);

            if (kullanici != null)
            {
                // Sadece değişmesine izin verdiğimiz alanları güncelliyoruz
                kullanici.AdSoyad = model.AdSoyad;
                kullanici.Telefon = model.Telefon;
                kullanici.Adres = model.Adres;
                kullanici.Sehir = model.Sehir;
                kullanici.Ilce = model.Ilce;
                kullanici.PostaKodu = model.PostaKodu;

                _context.SaveChanges();
                TempData["Message"] = "Profil bilgileriniz başarıyla güncellendi.";
            }

            return RedirectToAction("Profile");
        }
        // AccountController.cs içine ekle

        public IActionResult Orders()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login");

            int userId = int.Parse(userIdStr);

            // Durumu "Sepette" OLMAYAN her şeyi getiriyoruz (yani tamamlanmış siparişler)
            var siparisler = _context.Siparislers
                .Where(x => x.KullaniciId == userId && x.Durum != "Sepette")
                .OrderByDescending(x => x.SiparisTarihi)
                .ToList();

            return View(siparisler);
        }
    }
}