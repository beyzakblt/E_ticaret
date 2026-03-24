using System;
using System.Collections.Generic;

namespace E_ticaret.Models;

public partial class Kullanicilar
{
    public int Id { get; set; }

    public string? AdSoyad { get; set; }

    public string? Email { get; set; }

    public string? Sifre { get; set; }

    public string? Telefon { get; set; }

    public DateTime? KayitTarihi { get; set; }

    public bool? Durum { get; set; }

    public string? GoogleId { get; set; }

    public string? OnayKodu { get; set; }

    public string? Adres { get; set; }

    public string? Sehir { get; set; }

    public string? Ilce { get; set; }

    public string? PostaKodu { get; set; }

    public string? PasswordResetToken { get; set; }

    public DateTime? ResetTokenExpires { get; set; }
}
