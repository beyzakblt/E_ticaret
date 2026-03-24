using System;
using System.Collections.Generic;

namespace E_ticaret.Models;

public partial class Favoriler
{
    public int Id { get; set; }

    public int KullaniciId { get; set; }

    public int UrunId { get; set; }

    public DateTime? EklenmeTarihi { get; set; }
}
