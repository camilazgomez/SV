using System;
using System.Collections.Generic;

namespace SV.Models;

public partial class MultiOwner
{
    public int Id { get; set; }

    public string? Rut { get; set; }

    public double? OwnershipPercentage { get; set; }

    public string Commune { get; set; } = null!;

    public string Block { get; set; } = null!;

    public string Property { get; set; } = null!;

    public int Sheets { get; set; }

    public DateTime InscriptionDate { get; set; }

    public int InscriptionNumber { get; set; }

    public int Year { get; set; }

    public int validityYearBegin { get; set; }

    public int validityYearFinish { get; set; }

    public int? FormsId { get; set; }

    public virtual RealStateForm? Forms { get; set; }
}
