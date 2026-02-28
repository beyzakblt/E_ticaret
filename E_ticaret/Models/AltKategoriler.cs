using System;
using System.Collections.Generic;

namespace E_ticaret.Models;

public partial class AltKategoriler
{
    public int Id { get; set; }

    public string? KategoriAdi { get; set; }

    public int? AnaKategoriId { get; set; }

    public int? Durum { get; set; }

    public int? Kdv { get; set; }
}
