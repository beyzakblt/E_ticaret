using E_ticaret.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace E_ticaret.Controllers
{
    public class AdminController : Controller
    {
        private readonly EticaretContext _context;
        private readonly IWebHostEnvironment _env; // Tip 'object' değil, 'IWebHostEnvironment' olmalı

        // Constructor: Dışarıdan gelen servisleri içeriye alıyoruz
        public AdminController(EticaretContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // --- ŞİFRELEME ---
        private string SifreHashle(string sifre)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(sifre);
            var hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        // --- LOGIN İŞLEMLERİ ---
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string users, string pass)
        {
            if (string.IsNullOrEmpty(pass))
            {
                ViewBag.Error = "Şifre alanı boş bırakılamaz!";
                return View();
            }

            string hashedInput = SifreHashle(pass);
            var admin = _context.Yoneticilers.FirstOrDefault(x => x.Users == users && x.Pass == hashedInput);

            if (admin != null)
            {
                // Null Reference hatasını önlemek için ToString() ve ?? "" kullanımı
                HttpContext.Session.SetString("AdminId", admin.Id.ToString());
                HttpContext.Session.SetString("AdminUser", admin.Users ?? "Admin");
                return RedirectToAction("Index", "Admin");
            }

            ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
            return View();
        }

        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminId")))
                return RedirectToAction("Login");
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // --- SİTE AYARLARI (GET) ---
        [HttpGet]
        public IActionResult Site_Ayarlari()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminId")))
                return RedirectToAction("Login");

            // Eğer tabloda hiç veri yoksa hata vermemesi için kontrol
            var bilgi = _context.SiteAyarlaris.FirstOrDefault(x => x.Id == 1);
            if (bilgi == null)
            {
                bilgi = new SiteAyarlari { Id = 1 };
            }
            return View(bilgi);
        }
        [HttpPost]
        public async Task<IActionResult> Site_Ayarlari(
            string Site_Adi, string Tel, string Mail, string Whatsapp,
            string Adres, string Harita, string Facebook, string Instagram,
            string Twitter, string Linkedin, string Youtube, string Tiktok,
            string Pinteret, IFormFile? LogoFile)
        {
            // 1. ADIM: Mevcut kaydı bulmaya çalış (İlk kaydı getir)
            var guncel = _context.SiteAyarlaris.FirstOrDefault(); // ID sormadan direkt ilkini al

            // 2. ADIM: Eğer kayıt yoksa yeni oluştur (ID VERMEDEN!)
            if (guncel == null)
            {
                guncel = new SiteAyarlari();
                // Burada guncel.Id = 1; YAZMA! SQL kendisi verecek.
                _context.SiteAyarlaris.Add(guncel);
            }

            // 3. ADIM: Logo ve Klasör Kontrolü (Aynı kalıyor)
            if (LogoFile != null && LogoFile.Length > 0)
            {
                string klasorYolu = Path.Combine(_env.WebRootPath, "img");
                if (!Directory.Exists(klasorYolu)) { Directory.CreateDirectory(klasorYolu); }

                string dosyaAdi = Guid.NewGuid().ToString() + Path.GetExtension(LogoFile.FileName);
                string tamYol = Path.Combine(klasorYolu, dosyaAdi);

                using (var stream = new FileStream(tamYol, FileMode.Create))
                {
                    await LogoFile.CopyToAsync(stream);
                }
                guncel.Logo = "/img/" + dosyaAdi;
            }

            // 4. ADIM: Verileri Eşitle
            guncel.SiteAdi = Site_Adi;
            guncel.Tel = Tel;
            guncel.Mail = Mail;
            guncel.Whatsapp = Whatsapp;
            guncel.Adres = Adres;
            guncel.Harita = Harita;
            guncel.Facebook = Facebook;
            guncel.Instagram = Instagram;
            guncel.Twitter = Twitter;
            guncel.Linkedin = Linkedin;
            guncel.Youtube = Youtube;
            guncel.Tiktok = Tiktok;
            guncel.Pinteret = Pinteret;

            // 5. ADIM: Kaydet
            _context.SaveChanges();
            TempData["Success"] = "Ayarlar başarıyla güncellendi.";

            return RedirectToAction("Site_Ayarlari");
        }
    }
}