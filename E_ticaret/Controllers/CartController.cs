using Microsoft.AspNetCore.Mvc;
using E_ticaret.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace E_ticaret.Controllers
{
    public class CartController : Controller
    {
        private readonly EticaretContext _context;
        public CartController(EticaretContext context) { _context = context; }

        // YARDIMCI METOT: Session ID Yönetimi
        private string GetCartContextId()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (!string.IsNullOrEmpty(userId)) return userId;

                var guestId = HttpContext.Session.GetString("GuestId");
                if (string.IsNullOrEmpty(guestId))
                {
                    guestId = Guid.NewGuid().ToString();
                    HttpContext.Session.SetString("GuestId", guestId);
                }
                return guestId;
            }
            catch { return "temporary_guest"; }
        }

        // SEPET LİSTESİ
        public IActionResult Index()
        {
            string contextId = GetCartContextId();
            bool isRealUser = int.TryParse(contextId, out int userId);
            var userEmail = HttpContext.Session.GetString("UserEmail") ?? HttpContext.Session.GetString("TempUser_Email");

            var sepetim = _context.Siparislers
                 .Where(x => (isRealUser ? x.KullaniciId == userId : x.GuestId == contextId) && x.Durum == "Sepette")
                 .Include(x => x.Urun)
                 .ToList();

            if (!string.IsNullOrEmpty(userEmail))
            {
                ViewBag.KullaniciKuponlari = _context.Kuponlars
                    .Where(x => x.TanimliMail == userEmail && x.Durum == true && x.BitisTarihi > DateTime.Now)
                    .ToList();
            }

            return View(sepetim);
        }

        // ADET GÜNCELLEME
        [HttpPost]
        public IActionResult UpdateQuantity(int id, int change)
        {
            string contextId = GetCartContextId();
            bool isRealUser = int.TryParse(contextId, out int userId);
            var urun = _context.Urunlers.Find(id);
            if (urun == null) return Json(new { success = false });

            var sepetKaydi = _context.Siparislers.FirstOrDefault(x =>
                (isRealUser ? x.KullaniciId == userId : x.GuestId == contextId) &&
                x.UrunId == id && x.Durum == "Sepette");

            if (sepetKaydi == null && change > 0)
            {
                if (urun.Adet < 1) return Json(new { success = false, message = "Tükendi" });
                sepetKaydi = new Siparisler
                {
                    KullaniciId = isRealUser ? userId : (int?)null,
                    GuestId = isRealUser ? null : contextId,
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
                if (sepetKaydi.Adet <= 0) _context.Siparislers.Remove(sepetKaydi);
                else sepetKaydi.ToplamFiyat = sepetKaydi.Adet * urun.Fiyat;
            }
            _context.SaveChanges();

            int toplamAdet = _context.Siparislers.Where(x => (isRealUser ? x.KullaniciId == userId : x.GuestId == contextId) && x.Durum == "Sepette").Sum(x => (int?)x.Adet) ?? 0;
            return Json(new { success = true, yeniUrunAdet = sepetKaydi?.Adet ?? 0, toplamSepetSayisi = toplamAdet });
        }

        // ÖDEME ADIMI
        [HttpGet]
        public IActionResult Checkout()
        {
            string contextId = GetCartContextId();
            bool isRealUser = int.TryParse(contextId, out int userId);
            var sepet = _context.Siparislers.Where(x => (isRealUser ? x.KullaniciId == userId : x.GuestId == contextId) && x.Durum == "Sepette").Include(x => x.Urun).ToList();

            if (!sepet.Any()) return RedirectToAction("Index");

            decimal araToplam = sepet.Sum(x => x.ToplamFiyat ?? 0);
            int? indirimOrani = HttpContext.Session.GetInt32("IndirimOrani");

            if (indirimOrani.HasValue)
            {
                decimal indirimTutari = (araToplam * indirimOrani.Value) / 100;
                ViewBag.IndirimTutari = indirimTutari;
                ViewBag.GenelToplam = araToplam - indirimTutari;
            }
            else { ViewBag.GenelToplam = araToplam; }

            if (isRealUser) ViewBag.Kullanici = _context.Kullanicilars.Find(userId);
            return View(sepet);
        }

        // SİPARİŞİ TAMAMLA
        [HttpPost]
        public IActionResult CompleteOrder(string adSoyad, string email, string telefon, string adres)
        {
            string contextId = GetCartContextId();
            bool isRealUser = int.TryParse(contextId, out int userId);
            var sepet = _context.Siparislers.Where(x => (isRealUser ? x.KullaniciId == userId : x.GuestId == contextId) && x.Durum == "Sepette").ToList();

            if (!sepet.Any()) return RedirectToAction("Index");

            int? indirimOrani = HttpContext.Session.GetInt32("IndirimOrani");
            string yeniSiparisNo = "BZ" + DateTime.Now.ToString("yyyyMMddHHmm");

            foreach (var item in sepet)
            {
                var urun = _context.Urunlers.Find(item.UrunId);
                if (urun != null) urun.Adet -= item.Adet;

                if (indirimOrani.HasValue)
                {
                    item.ToplamFiyat = item.ToplamFiyat - (item.ToplamFiyat * indirimOrani.Value / 100);
                }

                item.Durum = "Sipariş Alındı";
                item.SiparisTarihi = DateTime.Now;
                item.SiparisNo = yeniSiparisNo;

                if (!isRealUser)
                {
                    item.MisafirAdSoyad = adSoyad;
                    item.MisafirEmail = email;
                    item.MisafirTelefon = telefon;
                    item.MisafirAdres = adres;
                }
            }

            try
            {
                _context.SaveChanges();
                if (!isRealUser) HttpContext.Session.Remove("GuestId");
                HttpContext.Session.Remove("IndirimOrani");
                HttpContext.Session.Remove("UygulananKupon");

                TempData["OrderSuccess"] = "Siparişiniz başarıyla oluşturuldu!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                return Content("Hata oluştu: " + ex.Message);
            }
        }

        // KUPON İŞLEMLERİ
        [HttpPost]
        public IActionResult KuponSorgula(string kod)
        {
            // 1. Giriş yapan kullanıcının mailini al (Kişiye özel kupon kontrolü için)
            var userEmail = HttpContext.Session.GetString("UserEmail");

            // 2. Kuponu veritabanında ara (Aktif olanları getir)
            var kupon = _context.Kuponlars.FirstOrDefault(x =>
                x.KuponKodu == kod.ToUpper() &&
                x.Durum == true);

            // --- KONTROLLER ---

            if (kupon == null)
                return Json(new { success = false, message = "Geçersiz veya pasif kupon kodu!" });

            if (kupon.BitisTarihi < DateTime.Now)
                return Json(new { success = false, message = "Bu kuponun süresi dolmuştur!" });

            if (kupon.KullanilanAdet >= kupon.KullanimLimiti)
                return Json(new { success = false, message = "Bu kuponun kullanım sınırı dolmuştur!" });

            // Kişiye özel mail kontrolü (Eğer TanimliMail alanı doluysa)
            if (!string.IsNullOrEmpty(kupon.TanimliMail) && kupon.TanimliMail.ToLower() != userEmail?.ToLower())
            {
                return Json(new { success = false, message = "Bu kupon sadece tanımlı e-posta adresi için geçerlidir!" });
            }

            // --- BAŞARILI ---
            // İndirim oranını Session'a atıyoruz ki sepeti hesaplarken fiyattan düşelim
            HttpContext.Session.SetInt32("IndirimOrani", kupon.IndirimOrani);
            HttpContext.Session.SetString("UygulananKupon", kupon.KuponKodu);

            return Json(new
            {
                success = true,
                message = $"Tebrikler! %{kupon.IndirimOrani} indirim uygulandı.",
                oran = kupon.IndirimOrani
            });
        }
        [HttpPost]
        public IActionResult KuponUygula(string kuponKodu)
        {
            if (string.IsNullOrEmpty(kuponKodu)) return Json(new { success = false, message = "Kod giriniz." });

            var kupon = _context.Kuponlars.FirstOrDefault(x => x.KuponKodu.ToLower() == kuponKodu.ToLower() && x.Durum == true && x.BitisTarihi >= DateTime.Now);
            if (kupon == null) return Json(new { success = false, message = "Geçersiz kupon!" });

            HttpContext.Session.SetInt32("IndirimOrani", kupon.IndirimOrani);
            HttpContext.Session.SetString("UygulananKupon", kupon.KuponKodu);
            return Json(new { success = true, message = $"%{kupon.IndirimOrani} uygulandı.", oran = kupon.IndirimOrani });
        }

        [HttpPost]
        public IActionResult KuponIptal()
        {
            HttpContext.Session.Remove("IndirimOrani");
            HttpContext.Session.Remove("UygulananKupon");
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int id)
        {
            var sepetKaydi = _context.Siparislers.Find(id);
            if (sepetKaydi != null) { _context.Siparislers.Remove(sepetKaydi); _context.SaveChanges(); return Json(new { success = true }); }
            return Json(new { success = false });
        }

        [HttpPost]
        public IActionResult SepeteEkleAjax(int id)
        {
            string contextId = GetCartContextId();
            bool isRealUser = int.TryParse(contextId, out int userId);
            var urun = _context.Urunlers.Find(id);
            if (urun == null) return Json(new { success = false });

            var sepetItem = _context.Siparislers.FirstOrDefault(x => (isRealUser ? x.KullaniciId == userId : x.GuestId == contextId) && x.UrunId == id && x.Durum == "Sepette");

            if (sepetItem != null) sepetItem.Adet++;
            else
            {
                _context.Siparislers.Add(new Siparisler
                {
                    UrunId = id,
                    Adet = 1,
                    ToplamFiyat = urun.Fiyat,
                    KullaniciId = isRealUser ? userId : (int?)null,
                    GuestId = isRealUser ? null : contextId,
                    Durum = "Sepette",
                    SiparisTarihi = DateTime.Now
                });
            }
            _context.SaveChanges();
            int yeniAdet = _context.Siparislers.Where(x => (isRealUser ? x.KullaniciId == userId : x.GuestId == contextId) && x.Durum == "Sepette").Sum(x => (int?)x.Adet) ?? 0;
            return Json(new { success = true, yeniAdet = yeniAdet });
        }
        [HttpGet]
        public IActionResult FaturaDetay(string siparisNo)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            // Sadece bu kullanıcıya ait ve bu sipariş numaralı ürünleri getir
            var faturaIcerik = _context.Siparislers
                .Where(x => x.KullaniciId == int.Parse(userId) && x.SiparisNo == siparisNo)
                .Include(x => x.Urun)
                .ToList();

            if (!faturaIcerik.Any()) return NotFound();

            // Fatura bilgilerini (Adres, Tarih vb.) ViewBag ile taşıyalım
            var ilkKayit = faturaIcerik.First();
            ViewBag.SiparisNo = siparisNo;
            ViewBag.Tarih = ilkKayit.SiparisTarihi;

            // Eğer veritabanında adres tutuyorsan buraya ekle
            ViewBag.Kullanici = _context.Kullanicilars.Find(int.Parse(userId));

            return View(faturaIcerik);
        }
    }
}