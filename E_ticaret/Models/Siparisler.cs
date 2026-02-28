using System;
using System.Collections.Generic;

namespace E_ticaret.Models;

public partial class Siparisler
{
    public int Id { get; set; }

    public int KullaniciId { get; set; }

    public int UrunId { get; set; }

    public int Adet { get; set; }

    public decimal? ToplamFiyat { get; set; }

    public DateTime? SiparisTarihi { get; set; }

    public string? Durum { get; set; }
}
