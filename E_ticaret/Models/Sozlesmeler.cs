using System;
using System.Collections.Generic;

namespace E_ticaret.Models;

public partial class Sozlesmeler
{
    public int Id { get; set; }

    public string Baslik { get; set; } = null!;

    public string Icerik { get; set; } = null!;

    public int SozlesmeTipi { get; set; }

    public DateTime? GuncellenmeTarihi { get; set; }
}
