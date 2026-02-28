using System;
using System.Collections.Generic;

namespace E_ticaret.Models;

public partial class Slider
{
    public int Id { get; set; }

    public string? Baslik { get; set; }

    public string? Aciklama { get; set; }

    public string? ResimYolu { get; set; }

    public int? Sira { get; set; }

    public bool Aktif { get; set; }

    public string? Link { get; set; }
}
