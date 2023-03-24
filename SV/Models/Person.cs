using System;
using System.Collections.Generic;

namespace SV.Models;

public partial class Person
{
    public int Id { get; set; }

    public string? Rut { get; set; }

    public double? OwnershipPercentage { get; set; }

    public bool? UncreditedOwnership { get; set; }

    public int? FormsId { get; set; }

    public bool? Seller { get; set; }

    public bool? Heir { get; set; }

    public virtual RealStateForm? Forms { get; set; }
}
