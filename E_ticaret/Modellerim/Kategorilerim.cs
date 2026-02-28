using E_ticaret.Models;

namespace E_ticaret.Modellerim
{
    public class Kategorilerim
    {
        // Sayfadaki listeler için
        public List<Kategoriler> AnaKategori { get; set; } = new List<Kategoriler>();
        public List<AltKategoriler> AltKategori { get; set; } = new List<AltKategoriler>();

        // Tablodaki her bir satır için (Join verileri)
        public int altid { get; set; }
        public string ad { get; set; }
        public string anakat_ad { get; set; }
        public int? durum { get; set; }
    }
    public class UrunView
    {
        // SQL Tablo kolonlarınla birebir uyumlu alanlar
        public int Id { get; set; }
        public string? Urun_Adi { get; set; }
        public int? Adet { get; set; }
        public decimal? Fiyat { get; set; }
        public decimal? Indirimli_Fiyat { get; set; }
        public string? Aciklama { get; set; }
        public int? Marka_Id { get; set; }
        public int? Kategori_Id { get; set; }
        public int? Foto_Id { get; set; }
        public int? Yorum_Id { get; set; }

        // --- Sayfa İçin Gerekli Listeler ---

        // Ürünleri tabloda listelemek için
        public List<Urunler> UrunListesi { get; set; } = new List<Urunler>();

        // Formdaki Select Box'ları doldurmak için
        public List<Kategoriler> KategoriListesi { get; set; } = new List<Kategoriler>();
        public List<Markalar> MarkaListesi { get; set; } = new List<Markalar>();

        // --- ÇOKLU FOTOĞRAF YÜKLEME ---
        // 'multiple' seçilen dosyaları bu liste yakalayacak
        public List<IFormFile>? UrunFotograflari { get; set; }
    }
}
