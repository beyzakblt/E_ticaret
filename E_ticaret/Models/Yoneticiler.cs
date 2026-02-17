using System;
using System.Collections.Generic;

namespace E_ticaret.Models;

public partial class Yoneticiler
{
    public int Id { get; set; }

    public string? Users { get; set; }

    public string? Pass { get; set; }

    public int? StatuId { get; set; }

    public int? Durum { get; set; }

    public int? IsActive { get; set; }

    public DateTime? SonGiris { get; set; }

    public int? InfoId { get; set; }
}
