using System;
using System.Collections.Generic;

namespace E_ticaret.Models;

public partial class Siparisler
{
    public int Id { get; set; }

    public int? KullaniciId { get; set; }

    public int UrunId { get; set; }

    public int Adet { get; set; }

    public decimal? ToplamFiyat { get; set; }

    public DateTime? SiparisTarihi { get; set; }

    public string? Durum { get; set; }

    public string? GuestId { get; set; }

    public string? IadeNedeni { get; set; }

    public DateTime? TalepTarihi { get; set; }

    public string? MisafirAdSoyad { get; set; }

    public string? MisafirEmail { get; set; }

    public string? MisafirTelefon { get; set; }

    public string? MisafirAdres { get; set; }

    public string? SiparisNo { get; set; }

    public int? IndirimOrani { get; set; }

    public string? UygulananKupon { get; set; }
    public virtual Urunler? Urun { get; set; }
    // Siparisler.cs içine ekle
    public virtual Kullanicilar? Kullanici { get; set; }
}
