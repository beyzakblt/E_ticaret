using System;
using System.Collections.Generic;

namespace E_ticaret.Models;

public partial class UrunYorumlari
{
    public int Id { get; set; }

    public int? KullaniciId { get; set; }

    public int? UrunId { get; set; }

    public string? Yorum { get; set; }

    public int? Puan { get; set; }
}
