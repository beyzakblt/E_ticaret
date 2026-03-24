using System;
using System.Collections.Generic;

namespace E_ticaret.Models;

public partial class StokHareketleri
{
    public int Id { get; set; }

    public int UrunId { get; set; }

    public decimal? EskiAlisFiyat { get; set; }

    public decimal? YeniAlisFiyat { get; set; }

    public int? AdetDegisimi { get; set; }

    public string? IslemTipi { get; set; }

    public DateTime KayitTarihi { get; set; }
}
