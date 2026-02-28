using E_ticaret.Modellerim;
using E_ticaret.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // --- YARDIMCI METOTLAR ---
        private string SifreHashle(string sifre)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(sifre);
            var hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        // --- LOGIN & LOGOUT ---
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

        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminId"))) return RedirectToAction("Login");
            return View();
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

        // --- API & SİTE AYARLARI (MEVCUT KODLARIN DEVAMI) ---
        [HttpGet]
        public IActionResult Api_Ayarlari() => View(_context.ApiAyarlaris.ToList());

        [HttpPost]
        public IActionResult Api_Ayarlari(string id, string ad, string kod)
        {
            if (id == "0") { _context.ApiAyarlaris.Add(new ApiAyarlari { Ad = ad, Kod = kod }); }
            else
            {
                var guncel = _context.ApiAyarlaris.Find(int.Parse(id));
                if (guncel != null) { guncel.Ad = ad; guncel.Kod = kod; }
            }
            _context.SaveChanges();
            return RedirectToAction("Api_Ayarlari");
        }

        public IActionResult Api_Sil(int id)
        {
            var sil = _context.ApiAyarlaris.Find(id);
            if (sil != null) { _context.ApiAyarlaris.Remove(sil); _context.SaveChanges(); }
            return RedirectToAction("Api_Ayarlari");
        }

        [HttpGet]
        public IActionResult Site_Ayarlari()
        {
            var bilgi = _context.SiteAyarlaris.FirstOrDefault() ?? new SiteAyarlari { Id = 1 };
            return View(bilgi);
        }

        [HttpPost]
        public async Task<IActionResult> Site_Ayarlari(SiteAyarlari model, IFormFile? LogoFile)
        {
            var guncel = _context.SiteAyarlaris.FirstOrDefault();
            if (guncel == null) { guncel = new SiteAyarlari(); _context.SiteAyarlaris.Add(guncel); }

            if (LogoFile != null)
            {
                string dosyaAdi = Guid.NewGuid().ToString() + Path.GetExtension(LogoFile.FileName);
                string yol = Path.Combine(_env.WebRootPath, "img", dosyaAdi);
                using (var s = new FileStream(yol, FileMode.Create)) { await LogoFile.CopyToAsync(s); }
                guncel.Logo = "/img/" + dosyaAdi;
            }

            guncel.SiteAdi = model.SiteAdi;
            guncel.Tel = model.Tel;
            guncel.Mail = model.Mail;
            guncel.Whatsapp = model.Whatsapp;
            guncel.Adres = model.Adres;
            guncel.Harita = model.Harita;
            _context.SaveChanges();
            return RedirectToAction("Site_Ayarlari");
        }
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
                // View tarafında kolaylık olması için listeyi dolduruyoruz
                UrunListesi = urunListesi.Select(x => x.u).ToList(),
                KategoriListesi = _context.Kategorilers.ToList(),
                MarkaListesi = _context.Markalars.ToList()
            };

            // İsimleri View tarafında "ViewBag" veya "TempData" ile taşıyabiliriz 
            // veya UrunView modeline 'MarkaAdlari' gibi bir sözlük ekleyebiliriz.
            ViewBag.KategoriAdlari = urunListesi.ToDictionary(x => x.u.Id, x => x.k?.KategoriAdi ?? "Yok");
            ViewBag.MarkaAdlari = urunListesi.ToDictionary(x => x.u.Id, x => x.m?.MarkaAd ?? "Yok");

            return View(model);
        }

        // --- ÜRÜN KAYDET (POST: EKLE & GÜNCELLE) ---
      
        [HttpPost]
        public async Task<IActionResult> UrunKaydet(UrunView model)
        {
            int urunId = model.Id;

            if (model.Id == 0) // YENİ EKLEME
            {
                var yeniUrun = new Urunler
                {
                    UrunAdi = model.Urun_Adi,
                    Adet = model.Adet ?? 0,
                    Fiyat = model.Fiyat ?? 0,
                    IndirimliFiyat = model.Indirimli_Fiyat,
                    Aciklama = model.Aciklama,
                    MarkaId = model.Marka_Id,
                    KategoriId = model.Kategori_Id
                };
                _context.Urunlers.Add(yeniUrun);
                await _context.SaveChangesAsync();
                urunId = yeniUrun.Id; // Yeni oluşan ID'yi aldık
            }
            else // GÜNCELLEME
            {
                var guncel = _context.Urunlers.Find(model.Id);
                if (guncel != null)
                {
                    guncel.UrunAdi = model.Urun_Adi;
                    guncel.Adet = model.Adet ?? 0;
                    guncel.Fiyat = model.Fiyat ?? 0;
                    guncel.IndirimliFiyat = model.Indirimli_Fiyat;
                    guncel.Aciklama = model.Aciklama;
                    guncel.MarkaId = model.Marka_Id;
                    guncel.KategoriId = model.Kategori_Id;
                    _context.Urunlers.Update(guncel);
                }
            }

            // --- FOTOĞRAFLARI KAYDETME ---
            if (model.UrunFotograflari != null && model.UrunFotograflari.Count > 0)
            {
                string klasorYolu = Path.Combine(_env.WebRootPath, "img", "urunler");
                if (!Directory.Exists(klasorYolu)) Directory.CreateDirectory(klasorYolu);

                foreach (var foto in model.UrunFotograflari)
                {
                    string dosyaAdi = Guid.NewGuid().ToString() + Path.GetExtension(foto.FileName);
                    string tamYol = Path.Combine(klasorYolu, dosyaAdi);

                    using (var stream = new FileStream(tamYol, FileMode.Create))
                    {
                        await foto.CopyToAsync(stream);
                    }

                    // --- VERİTABANINA KAYIT ---
                    // Senin modeline (UrunFoto) uygun kayıt yapıyoruz
                    var yeniUrunFoto = new UrunFoto
                    {
                        UrunId = urunId, // model.Id veya yeniUrun.Id
                        Ad = "/img/urunler/" + dosyaAdi
                    };
                    _context.UrunFotos.Add(yeniUrunFoto); // Context'teki ismi UrunFotos olabilir, kontrol et
                }
            }
            await _context.SaveChangesAsync();

            await _context.SaveChangesAsync();
            return RedirectToAction("Urunler");
        }

        // --- DÜZENLEME MODALINDA FOTOLARI GÖSTERMEK İÇİN YARDIMCI METOT ---
        [HttpGet]
        public IActionResult GetUrunFotolari(int id)
        {
            var fotolar = _context.UrunFotos
                .Where(x => x.UrunId == id)
                .Select(x => new { x.Id, x.Ad }) // Hem Id hem Ad (yol) gönderiyoruz
                .ToList();

            return Json(fotolar);
        }

        // --- ÜRÜN SİLME ---
        public IActionResult UrunSil(int id)
        {
            var silinecek = _context.Urunlers.Find(id);
            if (silinecek != null)
            {
                _context.Urunlers.Remove(silinecek);
                _context.SaveChanges();
            }
            return RedirectToAction("Urunler");
        }
        [HttpPost]
        public IActionResult FotoSil(int fotoId)
        {
            var foto = _context.UrunFotos.Find(fotoId);
            if (foto != null)
            {
                // 1. Fiziksel dosyayı klasörden sil
                string tamYol = Path.Combine(_env.WebRootPath, foto.Ad.TrimStart('/'));
                if (System.IO.File.Exists(tamYol))
                {
                    System.IO.File.Delete(tamYol);
                }

                // 2. Veritabanı kaydını sil
                _context.UrunFotos.Remove(foto);
                _context.SaveChanges();

                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // --- SÖZLEŞMELERİ GETİRME (GET) ---
        [HttpGet]
        public IActionResult Sozlesmeler(int id = 1)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminId"))) return RedirectToAction("Login");

            // Seçilen tipe göre veriyi getir, yoksa boş bir model oluştur
            var veri = _context.Sozlesmelers.FirstOrDefault(x => x.SozlesmeTipi == id);

            if (veri == null)
            {
                veri = new Sozlesmeler { SozlesmeTipi = id, Baslik = "", Icerik = "" };
            }

            ViewBag.TumSozlesmeler = _context.Sozlesmelers.ToList();
            ViewBag.AktifTip = id;
            return View(veri);
        }
        // --- SÖZLEŞME SİL ---
        public IActionResult SozlesmeSil(int id)
        {
            // Veritabanında o tipteki (veya ID'deki) kaydı bul
            var veri = _context.Sozlesmelers.FirstOrDefault(x => x.SozlesmeTipi == id);

            if (veri != null)
            {
                _context.Sozlesmelers.Remove(veri);
                _context.SaveChanges();
            }

            return RedirectToAction("Sozlesmeler");
        }

        // --- SÖZLEŞME KAYDET / GÜNCELLE (POST) ---
        [HttpPost]
        public IActionResult SozlesmeKaydet(Sozlesmeler model)
        {
            var guncellenecek = _context.Sozlesmelers.FirstOrDefault(x => x.SozlesmeTipi == model.SozlesmeTipi);

            if (guncellenecek != null)
            {
                // Güncelleme işlemi
                guncellenecek.Baslik = model.Baslik;
                guncellenecek.Icerik = model.Icerik;
                guncellenecek.GuncellenmeTarihi = DateTime.Now;
                _context.Update(guncellenecek);
            }
            else
            {
                // Yeni kayıt işlemi
                model.GuncellenmeTarihi = DateTime.Now;
                _context.Sozlesmelers.Add(model);
            }

            _context.SaveChanges();

            // Formu temizlemek için boş model gönderiyoruz
            ViewBag.AktifTip = model.SozlesmeTipi;
            var bosModel = new Sozlesmeler { SozlesmeTipi = model.SozlesmeTipi, Baslik = "", Icerik = "" };

            return View("Sozlesmeler", bosModel);
        }
        // --- SLIDER LİSTELEME ---
        public IActionResult Slider()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminId"))) return RedirectToAction("Login");

            var liste = _context.Sliders.OrderBy(x => x.Sira).ToList();
            return View(liste);
        }

        // --- SLIDER KAYDET (EKLE/GÜNCELLE) ---
        [HttpPost]
        public async Task<IActionResult> SliderKaydet(Slider model, IFormFile? ResimDosyasi)
        {
            if (ResimDosyasi != null)
            {
                // Resim yükleme işlemi
                string klasor = Path.Combine(_env.WebRootPath, "img", "slider");
                if (!Directory.Exists(klasor)) Directory.CreateDirectory(klasor);

                string dosyaAdi = Guid.NewGuid().ToString() + Path.GetExtension(ResimDosyasi.FileName);
                string tamYol = Path.Combine(klasor, dosyaAdi);

                using (var stream = new FileStream(tamYol, FileMode.Create))
                {
                    await ResimDosyasi.CopyToAsync(stream);
                }

                model.ResimYolu = "/img/slider/" + dosyaAdi;
            }

            if (model.Id == 0) // Yeni Kayıt
            {
                _context.Sliders.Add(model);
            }
            else // Güncelleme
            {
                var guncellenecek = _context.Sliders.Find(model.Id);
                if (guncellenecek != null)
                {
                    guncellenecek.Baslik = model.Baslik;
                    guncellenecek.Aciklama = model.Aciklama;
                    guncellenecek.Sira = model.Sira;
                    guncellenecek.Aktif = model.Aktif;
                    guncellenecek.Link = model.Link;
                    if (model.ResimYolu != null) guncellenecek.ResimYolu = model.ResimYolu;

                    _context.Update(guncellenecek);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Slider");
        }

        // --- SLIDER SİL ---
        public IActionResult SliderSil(int id)
        {
            var veri = _context.Sliders.Find(id);
            if (veri != null)
            {
                // Klasörden resmi de silelim (isteğe bağlı ama temizlik iyidir)
                if (!string.IsNullOrEmpty(veri.ResimYolu))
                {
                    string tamYol = Path.Combine(_env.WebRootPath, veri.ResimYolu.TrimStart('/'));
                    if (System.IO.File.Exists(tamYol)) System.IO.File.Delete(tamYol);
                }

                _context.Sliders.Remove(veri);
                _context.SaveChanges();
            }
            return RedirectToAction("Slider");
        }
        // --- KULLANICI LİSTESİ ---
        public IActionResult Kullanicilar()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminId"))) return RedirectToAction("Login");

            // Tüm üyeleri kayıt tarihine göre en yeni en üstte olacak şekilde getir
            var liste = _context.Kullanicilars.OrderByDescending(x => x.KayitTarihi).ToList();
            return View(liste);
        }

        // --- KULLANICI DURUMUNU DEĞİŞTİR (Engelini kaldır veya engelle) ---
        public IActionResult KullaniciDurumGuncelle(int id)
        {
            var kullanici = _context.Kullanicilars.Find(id);
            if (kullanici != null)
            {
                kullanici.Durum = !kullanici.Durum; // Aktifse pasif, pasifse aktif yap
                _context.SaveChanges();
            }
            return RedirectToAction("Kullanicilar");
        }
        // --- KULLANICI SİL ---
        public IActionResult KullaniciSil(int id)
        {
            var kullanici = _context.Kullanicilars.Find(id);
            if (kullanici != null)
            {
                _context.Kullanicilars.Remove(kullanici);
                _context.SaveChanges();
            }
            return RedirectToAction("Kullanicilar");
        }

        // --- KULLANICI GÜNCELLE (Şifresiz) ---
        [HttpPost]
        public IActionResult KullaniciGuncelle(Kullanicilar model)
        {
            var guncellenecek = _context.Kullanicilars.Find(model.Id);
            if (guncellenecek != null)
            {
                guncellenecek.AdSoyad = model.AdSoyad;
                guncellenecek.Email = model.Email;
                guncellenecek.Telefon = model.Telefon;
                // Şifreye hiç dokunmuyoruz, veritabanında olduğu gibi kalıyor

                _context.Update(guncellenecek);
                _context.SaveChanges();
            }
            return RedirectToAction("Kullanicilar");
        }
    }
}