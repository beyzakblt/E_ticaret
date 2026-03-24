using System;
using System.Collections.Generic;

namespace E_ticaret.Models;

public partial class Kuponlar
{
    public int Id { get; set; }

    public string KuponKodu { get; set; } = null!;

    public int IndirimOrani { get; set; }

    public int KullanimLimiti { get; set; }

    public int? KullanilanAdet { get; set; }

    public DateTime BitisTarihi { get; set; }

    public bool? Durum { get; set; }

    public string? TanimliMail { get; set; }
}
