using System;
using System.Collections.Generic;

namespace E_ticaret.Models;

public partial class Urunler
{
    public int Id { get; set; }

    public string UrunAdi { get; set; } = null!;

    public int Adet { get; set; }

    public decimal Fiyat { get; set; }

    public decimal? IndirimliFiyat { get; set; }

    public string? Aciklama { get; set; }

    public int? MarkaId { get; set; }

    public int? KategoriId { get; set; }

    public int? FotoId { get; set; }

    public int? YorumId { get; set; }
}
