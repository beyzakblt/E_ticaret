using ClosedXML.Excel;
using E_ticaret.Modellerim;
using E_ticaret.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace E_ticaret.Controllers
{
    public class AdminController : Controller
    {
        private readonly EticaretContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminController(EticaretContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ==========================================
        // 🛡️ GÜVENLİK KİLİDİ (URL'den Girişi Engeller)
        // ==========================================
        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            if (context.ActionDescriptor.RouteValues["action"] != "Login" &&
                string.IsNullOrEmpty(HttpContext.Session.GetString("AdminId")))
            {
                context.Result = RedirectToAction("Login", "Admin");
            }
            base.OnActionExecuting(context);
        }

        // --- YARDIMCI METOT: ŞİFRELEME ---
        private string SifreHashle(string sifre)
        {
            if (string.IsNullOrEmpty(sifre)) return "";
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(sifre);
            var hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        // --- GİRİŞ / ÇIKIŞ ---
        [HttpGet] public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string users, string pass)
        {
            if (string.IsNullOrEmpty(pass)) { ViewBag.Error = "Şifre boş olamaz!"; return View(); }
            string hashedInput = SifreHashle(pass);
            var admin = _context.Yoneticilers.FirstOrDefault(x => x.Users == users && x.Pass == hashedInput);
            if (admin != null)
            {
                HttpContext.Session.SetString("AdminId", admin.Id.ToString());
                HttpContext.Session.SetString("AdminUser", admin.Users ?? "Admin");
                return RedirectToAction("Index", "Admin");
            }
            ViewBag.Error = "Hatalı giriş!";
            return View();
        }

        public IActionResult Logout() { HttpContext.Session.Clear(); return RedirectToAction("Login"); }

        // --- 1. DASHBOARD (ANALİZ) ---
        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminId"))) return RedirectToAction("Login");
            ViewBag.AdminName = HttpContext.Session.GetString("AdminUser");

            var tumSiparisler = _context.Siparislers.Where(x => x.Durum != "Sepette").ToList();

            ViewBag.UrunSayisi = _context.Urunlers.Count();
            ViewBag.MusteriSayisi = _context.Kullanicilars.Count();

            ViewBag.ToplamSiparis = tumSiparisler.Where(x => !string.IsNullOrEmpty(x.SiparisNo)).GroupBy(x => x.SiparisNo).Count();
            ViewBag.TamamlananSiparis = tumSiparisler.Count(x => x.Durum == "Teslim Edildi");
            ViewBag.IptalSiparis = tumSiparisler.Count(x => x.Durum == "İptal Edildi" || x.Durum == "İade Reddedildi");
            ViewBag.BekleyenSiparis = tumSiparisler.Count(x => x.Durum == "Sipariş Alındı" || x.Durum == "Hazırlanıyor" || x.Durum == "İade Talebi Oluşturuldu");
            ViewBag.ToplamKazanc = tumSiparisler.Where(x => x.Durum != "İptal Edildi").Sum(x => x.ToplamFiyat ?? 0);

            return View();
        }

        // --- 2. YÖNETİCİLER VE KULLANICILAR ---
        public IActionResult Yoneticiler() => View(_context.Yoneticilers.ToList());

        [HttpPost]
        public IActionResult YoneticiEkle(string Users, string Pass, int StatuId)
        {
            _context.Yoneticilers.Add(new Yoneticiler { Users = Users, Pass = SifreHashle(Pass), StatuId = StatuId, Durum = 1, IsActive = 1, SonGiris = DateTime.Now });
            _context.SaveChanges();
            return RedirectToAction("Yoneticiler");
        }

        public IActionResult YoneticiSil(int id)
        {
            var a = _context.Yoneticilers.Find(id);
            if (a != null) { _context.Yoneticilers.Remove(a); _context.SaveChanges(); }
            return RedirectToAction("Yoneticiler");
        }

        public IActionResult Kullanicilar() => View(_context.Kullanicilars.OrderByDescending(x => x.KayitTarihi).ToList());

        public IActionResult KullaniciSil(int id)
        {
            var k = _context.Kullanicilars.Find(id);
            if (k != null) { _context.Kullanicilars.Remove(k); _context.SaveChanges(); }
            return RedirectToAction("Kullanicilar");
        }

        // --- KATEGORİ YÖNETİMİ (GET) ---
        [HttpGet]
        public IActionResult Kategoriler()
        {
            // Önce ana kategorileri çek
            var anaKategoriler = _context.Kategorilers.ToList();

            // Eğer hiç ana kategori yoksa, View tarafında hata almamak için 
            // boş bir listeyi de içeren bir Kategorilerim nesnesi dönmeliyiz.
            if (!anaKategoriler.Any())
            {
                // Boş bir liste döndür ki "Yeni Ana Kategori" butonu en azından çalışsın
                return View(new List<Kategorilerim> { new Kategorilerim { AnaKategori = new List<Kategoriler>() } });
            }

            var model = _context.AltKategorilers.Join(
                _context.Kategorilers,
                alt => alt.AnaKategoriId,
                ana => ana.Id,
                (alt, ana) => new Kategorilerim
                {
                    altid = alt.Id,
                    ad = alt.KategoriAdi,
                    durum = alt.Durum,
                    anakat_ad = ana.KategoriAdi,
                    AnaKategori = anaKategoriler
                }).ToList();

            return View(model);
        }

        // --- ANA KATEGORİ İŞLEMLERİ ---
        [HttpPost]
        public IActionResult AnaKategoriEkle(string KategoriAdi, int Durum)
        {
            var yeni = new Kategoriler { KategoriAdi = KategoriAdi, Durum = Durum };
            _context.Kategorilers.Add(yeni);
            _context.SaveChanges();
            return RedirectToAction("Kategoriler");
        }

        [HttpPost]
        public IActionResult AnaKategoriGuncelle(int anaid, string ad, int durum)
        {
            var guncel = _context.Kategorilers.Find(anaid);
            if (guncel != null)
            {
                guncel.KategoriAdi = ad;
                guncel.Durum = durum;
                _context.SaveChanges();
            }
            return RedirectToAction("Kategoriler");
        }

        public IActionResult AnaKategoriSil(int id)
        {
            var veri = _context.Kategorilers.Find(id);
            if (veri != null) { _context.Kategorilers.Remove(veri); _context.SaveChanges(); }
            return RedirectToAction("Kategoriler");
        }

        // --- ALT KATEGORİ İŞLEMLERİ ---
        [HttpPost]
        public IActionResult AltKategoriEkle(int AnaKategoriId, string KategoriAdi, int Durum)
        {
            var yeni = new AltKategoriler { AnaKategoriId = AnaKategoriId, KategoriAdi = KategoriAdi, Durum = Durum };
            _context.AltKategorilers.Add(yeni);
            _context.SaveChanges();
            return RedirectToAction("Kategoriler");
        }

        [HttpPost]
        public IActionResult AltKategoriGuncelle(int altid, string ad, int durum)
        {
            var guncel = _context.AltKategorilers.Find(altid);
            if (guncel != null)
            {
                guncel.KategoriAdi = ad;
                guncel.Durum = durum;
                _context.SaveChanges();
            }
            return RedirectToAction("Kategoriler");
        }

        public IActionResult AltKategoriSil(int id)
        {
            var veri = _context.AltKategorilers.Find(id);
            if (veri != null) { _context.AltKategorilers.Remove(veri); _context.SaveChanges(); }
            return RedirectToAction("Kategoriler");
        }


        // --- 4. ÜRÜN YÖNETİMİ ---
        [HttpGet]
        public IActionResult Urunler()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminId"))) return RedirectToAction("Login");

            var urunListesi = (from u in _context.Urunlers
                               join k in _context.Kategorilers on u.KategoriId equals k.Id into katGrup
                               from k in katGrup.DefaultIfEmpty()
                               join m in _context.Markalars on u.MarkaId equals m.Id into markaGrup
                               from m in markaGrup.DefaultIfEmpty()
                               select new { u, k, m }).ToList();

            var model = new UrunView
            {
                UrunListesi = urunListesi.Select(x => x.u).ToList(),
                KategoriListesi = _context.Kategorilers.ToList(),
                MarkaListesi = _context.Markalars.ToList()
            };

            ViewBag.KategoriAdlari = urunListesi.ToDictionary(x => x.u.Id, x => x.k?.KategoriAdi ?? "Yok");
            ViewBag.MarkaAdlari = urunListesi.ToDictionary(x => x.u.Id, x => x.m?.MarkaAd ?? "Yok");

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UrunKaydet(Urunler model, List<IFormFile> UrunFotograflari)
        {
            // Eğer modelden gelen UrunAdi boşsa, hata vermemesi için kontrol ekleyelim
            if (string.IsNullOrEmpty(model.UrunAdi))
            {
                // Eğer buraya giriyorsa, formdaki 'name' alanı 'UrunAdi' DEĞİLDİR.
                return Content("HATA: Formdan ürün adı gelmedi! Lütfen input name alanını kontrol et.");
            }

            Urunler u = model.Id == 0 ? new Urunler() : _context.Urunlers.Find(model.Id) ?? new Urunler();

            // Verileri aktarırken isimlerin birebir tuttuğundan emin ol
            u.UrunAdi = model.UrunAdi;
            u.Adet = model.Adet;
            u.Fiyat = model.Fiyat;
            u.AlisFiyati = model.AlisFiyati;
            u.KritikStok = model.KritikStok;
            // ... diğer alanlar ...

            if (model.Id == 0) _context.Urunlers.Add(u);
            else _context.Urunlers.Update(u);

            await _context.SaveChangesAsync(); // Hata burada patlıyordu, artık dolu gidecek.

            return RedirectToAction("Urunler");
        }

        public IActionResult UrunSil(int id)
        {
            var sil = _context.Urunlers.Find(id);
            if (sil != null) { _context.Urunlers.Remove(sil); _context.SaveChanges(); }
            return RedirectToAction("Urunler");
        }

        // --- 5. SİPARİŞ VE İADE ---
        public IActionResult Siparis_Yonetimi() => View(_context.Siparislers.Where(x => x.Durum != "Sepette").OrderByDescending(x => x.SiparisTarihi).ToList());

        [HttpPost]
        public IActionResult GuncelleSiparisDurum(int id, string yeniDurum)
        {
            var s = _context.Siparislers.Find(id);
            if (s != null)
            {
                // Eğer sipariş iptal ediliyorsa stoğu geri yükle
                if (yeniDurum == "İptal Edildi" && s.Durum != "İptal Edildi")
                {
                    var urun = _context.Urunlers.Find(s.UrunId);
                    if (urun != null) urun.Adet += s.Adet;
                }

                s.Durum = yeniDurum;
                _context.SaveChanges();
            }
            return RedirectToAction("Siparis_Yonetimi");
        }

        [HttpPost]
        public IActionResult IadeOnayla(int id, bool onay)
        {
            var siparis = _context.Siparislers.Find(id);
            if (siparis != null)
            {
                if (onay)
                {
                    siparis.Durum = "İade Onaylandı";
                    var urun = _context.Urunlers.Find(siparis.UrunId);
                    if (urun != null) urun.Adet += siparis.Adet;
                }
                else siparis.Durum = "İade Reddedildi";
                _context.SaveChanges();
            }
            return RedirectToAction("Siparis_Yonetimi");
        }

        // --- 6. AYARLAR VE MESAJLAR ---
        public IActionResult Site_Ayarlari() => View(_context.SiteAyarlaris.FirstOrDefault() ?? new SiteAyarlari());

        [HttpPost]
        public async Task<IActionResult> Site_Ayarlari(SiteAyarlari m, IFormFile? LogoFile)
        {
            var g = _context.SiteAyarlaris.FirstOrDefault() ?? new SiteAyarlari();
            if (LogoFile != null)
            {
                string ad = Guid.NewGuid().ToString() + Path.GetExtension(LogoFile.FileName);
                using (var s = new FileStream(Path.Combine(_env.WebRootPath, "img", ad), FileMode.Create)) { await LogoFile.CopyToAsync(s); }
                g.Logo = "/img/" + ad;
            }
            g.SiteAdi = m.SiteAdi; g.Tel = m.Tel; g.Mail = m.Mail; g.Whatsapp = m.Whatsapp; g.Adres = m.Adres;
            if (g.Id == 0) _context.SiteAyarlaris.Add(g); else _context.SiteAyarlaris.Update(g);
            _context.SaveChanges(); return RedirectToAction("Site_Ayarlari");
        }

        public IActionResult Api_Ayarlari() => View(_context.ApiAyarlaris.ToList());

        [HttpPost]
        public IActionResult Api_Ayarlari(string id, string ad, string kod)
        {
            if (id == "0") _context.ApiAyarlaris.Add(new ApiAyarlari { Ad = ad, Kod = kod });
            else { var g = _context.ApiAyarlaris.Find(int.Parse(id)); if (g != null) { g.Ad = ad; g.Kod = kod; } }
            _context.SaveChanges(); return RedirectToAction("Api_Ayarlari");
        }

        public IActionResult GelenKutusu() => View(_context.ContactMessages.OrderByDescending(x => x.CreatedAt).ToList());

        // --- 7. MAİL GÖNDERME ---
        [HttpGet]
        public IActionResult MailGonder(string? targetEmail)
        {
            ViewBag.Kullanicilar = _context.Kullanicilars.Where(x => x.Durum == true).ToList();
            ViewBag.TargetEmail = targetEmail;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> MailGonder(string[] secilenMailler, bool tumuneGonder, string konu, string mesaj)
        {
            var alicilar = tumuneGonder ? _context.Kullanicilars.Where(x => x.Durum == true).Select(x => x.Email).ToList() : secilenMailler?.ToList();
            if (alicilar == null || alicilar.Count == 0) return RedirectToAction("MailGonder");

            using (var smtp = new SmtpClient("smtp.gmail.com", 587) { EnableSsl = true, Credentials = new NetworkCredential("beyzakblt@gmail.com", "eqvm lqjb ubnm hdok") })
            {
                foreach (var email in alicilar)
                {
                    if (string.IsNullOrEmpty(email)) continue;
                    var m = new MailMessage("beyzakblt@gmail.com", email, konu, mesaj) { IsBodyHtml = true };
                    await smtp.SendMailAsync(m);
                }
            }
            return RedirectToAction("MailGonder");
        }

        // --- 8. SLIDER VE KUPONLAR ---
        public IActionResult Slider() => View(_context.Sliders.OrderBy(x => x.Sira).ToList());

        [HttpPost]
        public async Task<IActionResult> SliderKaydet(Slider model, IFormFile? ResimDosyasi)
        {
            if (ResimDosyasi != null)
            {
                string ad = Guid.NewGuid().ToString() + Path.GetExtension(ResimDosyasi.FileName);
                string yol = Path.Combine(_env.WebRootPath, "img", "slider");
                if (!Directory.Exists(yol)) Directory.CreateDirectory(yol);
                using (var s = new FileStream(Path.Combine(yol, ad), FileMode.Create)) { await ResimDosyasi.CopyToAsync(s); }
                model.ResimYolu = "/img/slider/" + ad;
            }
            if (model.Id == 0) _context.Sliders.Add(model);
            else { var g = _context.Sliders.Find(model.Id); if (g != null) { g.Baslik = model.Baslik; g.Sira = model.Sira; g.Aktif = model.Aktif; if (model.ResimYolu != null) g.ResimYolu = model.ResimYolu; } }
            _context.SaveChanges(); return RedirectToAction("Slider");
        }

        public IActionResult SliderSil(int id) { var v = _context.Sliders.Find(id); if (v != null) { _context.Sliders.Remove(v); _context.SaveChanges(); } return RedirectToAction("Slider"); }

        // --- 1. KUPONLARI LİSTELE (Senin verdiğin satır) ---
        public IActionResult Kuponlar() => View(_context.Kuponlars.ToList());

        // --- 2. YENİ KUPON EKLE ---
        [HttpPost]
        public IActionResult KuponEkle(Kuponlar model)
        {
            if (ModelState.IsValid)
            {
                // Kodları standart olarak büyük harfe çevirelim
                model.KuponKodu = model.KuponKodu.ToUpper();
                model.Durum = true; // Kuponu aktif yap
                model.KullanilanAdet = 0; // Yeni kuponun kullanımı 0'dan başlar

                _context.Kuponlars.Add(model);
                _context.SaveChanges();
            }
            return RedirectToAction("Kuponlar");
        }

        // --- 3. KUPON SİL ---
        public IActionResult KuponSil(int id)
        {
            var kupon = _context.Kuponlars.Find(id);
            if (kupon != null)
            {
                _context.Kuponlars.Remove(kupon);
                _context.SaveChanges();
            }
            return RedirectToAction("Kuponlar");
        }

        // --- 4. KUPON DURUM GÜNCELLE (Aktif/Pasif Yap) ---
        public IActionResult KuponDurumGuncelle(int id)
        {
            var kupon = _context.Kuponlars.Find(id);
            if (kupon != null)
            {
                kupon.Durum = !kupon.Durum; // Aktifse pasif, pasifse aktif yapar
                _context.SaveChanges();
            }
            return RedirectToAction("Kuponlar");
        }

        // --- 9. SÖZLEŞMELER VE RAPOR ---
        public IActionResult Sozlesmeler(int id = 1)
        {
            ViewBag.TumSozlesmeler = _context.Sozlesmelers.ToList();
            ViewBag.AktifTip = id;
            return View(_context.Sozlesmelers.FirstOrDefault(x => x.SozlesmeTipi == id) ?? new Sozlesmeler { SozlesmeTipi = id });
        }

        [HttpPost]
        public IActionResult SozlesmeKaydet(Sozlesmeler model)
        {
            var g = _context.Sozlesmelers.FirstOrDefault(x => x.SozlesmeTipi == model.SozlesmeTipi);
            if (g != null) { g.Baslik = model.Baslik; g.Icerik = model.Icerik; g.GuncellenmeTarihi = DateTime.Now; }
            else { model.GuncellenmeTarihi = DateTime.Now; _context.Sozlesmelers.Add(model); }
            _context.SaveChanges(); return RedirectToAction("Sozlesmeler", new { id = model.SozlesmeTipi });
        }

        public IActionResult RaporYazdir()
        {
            var veriler = _context.Siparislers.ToList();
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Rapor");
            ws.Cell(1, 1).Value = "ID"; ws.Cell(1, 2).Value = "Durum"; ws.Cell(1, 3).Value = "Tutar";
            for (int i = 0; i < veriler.Count; i++) { ws.Cell(i + 2, 1).Value = veriler[i].Id; ws.Cell(i + 2, 2).Value = veriler[i].Durum; ws.Cell(i + 2, 3).Value = veriler[i].ToplamFiyat; }
            using var ms = new MemoryStream(); wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Rapor.xlsx");
        }

    }
}