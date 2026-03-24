using System;
using System.Collections.Generic;

namespace E_ticaret.Models;

public partial class KullaniciAdresleri
{
    public int Id { get; set; }

    public int KullaniciId { get; set; }

    public string? AdresBasligi { get; set; }

    public string? AdSoyad { get; set; }

    public string? Telefon { get; set; }

    public string? Sehir { get; set; }

    public string? Ilce { get; set; }

    public string? TamAdres { get; set; }

    public string? PostaKodu { get; set; }

    public bool? IsDefault { get; set; }
}
