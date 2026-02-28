using System;
using System.Collections.Generic;

namespace E_ticaret.Models;

public partial class UrunFoto
{
    public int Id { get; set; }

    public string? Ad { get; set; }

    public int? UrunId { get; set; }
}
