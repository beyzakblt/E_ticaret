using E_ticaret.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http; // Session için mutlaka olmalı
using System.Diagnostics;
using System.Net;
using System.Net.Mail;

namespace E_ticaret.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EticaretContext _context;

        // IConfiguration'ı kullanmadığın için kaldırdık, açılış hatasını engeller
        public HomeController(ILogger<HomeController> logger, EticaretContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index(string q)
        {
            // Veritabanındaki Aktif sliderları Sıra numarasına göre çekiyoruz
            var sliderListesi = _context.Sliders
                .Where(x => x.Aktif == true)
                .OrderBy(x => x.Sira)
                .ToList();

            // Sliderları View'a göndermek için ViewBag kullanıyoruz
            ViewBag.Sliders = sliderListesi;

            // Ürün listeleme kısmın (mevcut kodun)
            var urunler = _context.Urunlers.AsQueryable();
            if (!string.IsNullOrEmpty(q))
            {
                urunler = urunler.Where(x => x.UrunAdi.Contains(q) || x.Aciklama.Contains(q));
                ViewBag.SearchQuery = q;
            }

            return View(urunler.ToList());
        }

        public IActionResult UrunDetay(int id)
        {
            var urun = _context.Urunlers.FirstOrDefault(x => x.Id == id);
            if (urun == null) return RedirectToAction("Index");

            var userId = HttpContext.Session.GetString("UserId");
            var guestId = HttpContext.Session.GetString("GuestId");

            int sepetAdet = 0;
            if (!string.IsNullOrEmpty(userId))
            {
                sepetAdet = _context.Siparislers.FirstOrDefault(x => x.KullaniciId == int.Parse(userId) && x.UrunId == id && x.Durum == "Sepette")?.Adet ?? 0;
            }
            else if (!string.IsNullOrEmpty(guestId))
            {
                sepetAdet = _context.Siparislers.FirstOrDefault(x => x.GuestId == guestId && x.UrunId == id && x.Durum == "Sepette")?.Adet ?? 0;
            }

            ViewBag.SepetAdet = sepetAdet;
            return View(urun);
        }

        // --- İLETİŞİM SAYFASI (GET) ---
        [HttpGet]
        public IActionResult Contact()
        {
            // View bir model beklediği için ayarları gönderiyoruz
            var ayarlar = _context.SiteAyarlaris.FirstOrDefault() ?? new SiteAyarlari();
            return View(ayarlar);
        }

        // --- İLETİŞİM FORMU GÖNDERME (POST) ---
        [HttpPost]
        public IActionResult Contact(string fullName, string email, string subject, string message)
        {
            var ayarlar = _context.SiteAyarlaris.FirstOrDefault() ?? new SiteAyarlari();

            try
            {
                var contact = new ContactMessage
                {
                    FullName = fullName,
                    Email = email,
                    Subject = subject,
                    Message = message,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };

                _context.ContactMessages.Add(contact);
                _context.SaveChanges();

                // Mail gönderimi
                using (var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    Credentials = new NetworkCredential("beyzakblt@gmail.com", "eqvm lqjb ubnm hdok")
                })
                {
                    var mailMessage = new MailMessage("beyzakblt@gmail.com", "beyzakblt@gmail.com")
                    {
                        Subject = "İletişim Formu: " + subject,
                        Body = $"Gönderen: {fullName}\nE-posta: {email}\nKonu: {subject}\nMesaj: {message}",
                        IsBodyHtml = true
                    };
                    smtp.Send(mailMessage);
                }

                ViewBag.Success = "Mesajınız başarıyla iletildi.";
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Mesaj kaydedildi ancak mail gönderilemedi: " + ex.Message;
            }

            return View(ayarlar); // Ayarları geri gönderiyoruz ki sayfa bozulmasın
        }

        public IActionResult Hakkimizda()
        {
            return View();
        }

        [HttpPost]
        public IActionResult FavoriIslem(int urunId)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return Json(new { success = false, message = "Lütfen önce giriş yapın!" });

            int userId = int.Parse(userIdStr);
            var varMi = _context.Favorilers.FirstOrDefault(x => x.KullaniciId == userId && x.UrunId == urunId);

            if (varMi != null)
            {
                _context.Favorilers.Remove(varMi);
            }
            else
            {
                _context.Favorilers.Add(new Favoriler { KullaniciId = userId, UrunId = urunId });
            }
            _context.SaveChanges();
            return Json(new { success = true });
        }

        public IActionResult Favorilerim()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdStr);
            var favoriUrunler = _context.Favorilers
                .Where(x => x.KullaniciId == userId)
                .Join(_context.Urunlers, f => f.UrunId, u => u.Id, (f, u) => u)
                .ToList();

            return View(favoriUrunler);
        }
        // HomeController.cs içine ekle
        public IActionResult SSS()
        {
            // Statik bir liste oluşturuyoruz, istersen ileride bunu veritabanına bağlarız
            return View();
        }
        // --- SİPARİŞ TAKİP SAYFASINI AÇ (GET) ---
        [HttpGet] // Bu mutlaka olmalı
        public IActionResult SiparisTakip()
        {
            return View();
        }
        // --- SİPARİŞ SORGULAMA SONUCU (POST) ---
        [HttpPost]
        public IActionResult SiparisTakipSonuc(string siparisNo, string email)
        {
            // 1. ADIM: Sayı olan KullaniciId yerine, nesne olan 'Kullanici'yı Include ediyoruz
            var siparisler = _context.Siparislers
                .Include(x => x.Kullanici) // ID değil, tablo bağını (Navigation Property) ekle
                .Where(x => x.SiparisNo == siparisNo &&
                            (x.Kullanici.Email == email || x.MisafirEmail == email) && // Hem üye hem misafir mailini kontrol eder
                            x.Durum != "Sepette")
                .ToList();

            if (siparisler.Any())
            {
                // Ürün fotoğrafları için Urunler tablosunu da listeye dahil edelim
                ViewBag.Urunler = _context.Urunlers.ToList();
                return View(siparisler);
            }

            TempData["Error"] = "Sipariş numarası veya e-posta hatalı!";
            return RedirectToAction("SiparisTakip");
        }
    }
}