using Microsoft.AspNetCore.Mvc;
using E_ticaret.Models;
using Microsoft.EntityFrameworkCore;

namespace E_ticaret.Controllers
{
    public class CartController : Controller
    {
        private readonly EticaretContext _context;
        public CartController(EticaretContext context) { _context = context; }

        // SEPETİ GÖRÜNTÜLEME SAYFASI
        public IActionResult Index()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdString);

            // DİKKAT: Burada Siparisler listesi gönderiyoruz
            var sepetim = _context.Siparislers
                .Where(x => x.KullaniciId == userId && x.Durum == "Sepette")
                .ToList();

            return View(sepetim); // Sayfaya 'sepetim' (Siparisler listesi) gidiyor
        }

        [HttpPost]
        public IActionResult AddToCart(int urunId)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
                return Json(new { success = false });

            int userId = int.Parse(userIdString);

            var urun = _context.Urunlers.Find(urunId);
            if (urun != null)
            {
                Siparisler yeni = new Siparisler
                {
                    KullaniciId = userId,
                    UrunId = urun.Id,
                    Adet = 1,
                    ToplamFiyat = urun.IndirimliFiyat > 0 ? urun.IndirimliFiyat : urun.Fiyat,
                    SiparisTarihi = DateTime.Now,
                    Durum = "Sepette"
                };
                _context.Siparislers.Add(yeni);
                _context.SaveChanges();
            }

            // Güncel sepet sayısını al (Sadece "Sepette" olanları say)
            int sepetSayisi = _context.Siparislers.Count(x => x.KullaniciId == userId && x.Durum == "Sepette");

            // Sayfa döndürme, sadece JSON verisi döndür!
            return Json(new { success = true, count = sepetSayisi });
        }
        [HttpPost]
        public IActionResult UpdateQuantity(int id, int change)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return Json(new { success = false });

            int userId = int.Parse(userIdStr);

            // 1. İlgili sipariş satırını bul
            var sepetKaydi = _context.Siparislers.FirstOrDefault(x => x.KullaniciId == userId && x.UrunId == id && x.Durum == "Sepette");

            if (sepetKaydi == null && change > 0)
            {
                var urun = _context.Urunlers.Find(id);
                sepetKaydi = new Siparisler
                {
                    KullaniciId = userId,
                    UrunId = id,
                    Adet = 1,
                    ToplamFiyat = urun.Fiyat,
                    Durum = "Sepette",
                    SiparisTarihi = DateTime.Now
                };
                _context.Siparislers.Add(sepetKaydi);
            }
            else if (sepetKaydi != null)
            {
                sepetKaydi.Adet += change;

                // Eğer adet 0'a düşerse satırı tamamen sil
                if (sepetKaydi.Adet <= 0)
                {
                    _context.Siparislers.Remove(sepetKaydi);
                }
                else
                {
                    // Fiyatı da güncelle (Adet * Birim Fiyat)
                    var urun = _context.Urunlers.Find(id);
                    sepetKaydi.ToplamFiyat = sepetKaydi.Adet * (urun.IndirimliFiyat > 0 ? urun.IndirimliFiyat : urun.Fiyat);
                }
            }

            _context.SaveChanges();

            // --- BURASI KRİTİK: Navbardaki toplam sayıyı hesapla ---
            // Count() yerine Sum(x => x.Adet) kullanıyoruz. 
            // Yani 2 ekmek + 1 süt = 3 yazar.
            int toplamAdet = _context.Siparislers
                .Where(x => x.KullaniciId == userId && x.Durum == "Sepette")
                .Sum(x => (int?)x.Adet) ?? 0;

            return Json(new
            {
                success = true,
                yeniUrunAdet = sepetKaydi?.Adet ?? 0,
                toplamSepetSayisi = toplamAdet
            });
        }
        [HttpPost]
        public IActionResult RemoveFromCart(int id) // Buradaki 'id' ismi JavaScript'teki { id: siparisId } ile aynı olmalı
        {
            var siparis = _context.Siparislers.Find(id);
            if (siparis != null)
            {
                _context.Siparislers.Remove(siparis);
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
        // CartController.cs

        [HttpGet]
        public IActionResult Checkout()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdStr);

            // Kullanıcının adres bilgilerini çekiyoruz
            var kullanici = _context.Kullanicilars.Find(userId);

            // Kullanıcının sepetindeki ürünleri çekiyoruz
            var sepet = _context.Siparislers
                .Where(x => x.KullaniciId == userId && x.Durum == "Sepette")
                .ToList();

            if (!sepet.Any()) return RedirectToAction("Index");

            // Bilgileri ekranda göstermek için ViewBag kullanabiliriz veya bir ViewModel yapabiliriz
            ViewBag.Kullanici = kullanici;
            return View(sepet);
        }

        [HttpPost]
        public IActionResult CompleteOrder()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            int userId = int.Parse(userIdStr);

            // Sepetteki ürünleri bul ve durumlarını güncelle
            var sepet = _context.Siparislers.Where(x => x.KullaniciId == userId && x.Durum == "Sepette").ToList();

            foreach (var item in sepet)
            {
                item.Durum = "Sipariş Alındı"; // "Sepette" durumundan çıkardık
                item.SiparisTarihi = DateTime.Now;
            }

            _context.SaveChanges();

            // Sipariş bittiğinde bir teşekkür sayfasına veya siparişlerime yönlendir
            TempData["OrderSuccess"] = "Siparişiniz başarıyla alındı! Kapıda ödeme ile kapınızda.";
            return RedirectToAction("Index", "Home");
        }
    }
}