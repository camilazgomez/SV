using NuGet.Protocol.Plugins;
using System;
using System.Collections.Generic;

namespace SV.Models;

public partial class MultiOwner
{
    public MultiOwner()
    {
    }
    public int Id { get; set; }

    public string? Rut { get; set; }

    public double? OwnershipPercentage { get; set; }

    public string Commune { get; set; } = null!;

    public string Block { get; set; } = null!;

    public string Property { get; set; } = null!;

    public int? Sheets { get; set; }

    public DateTime InscriptionDate { get; set; }

    public int? InscriptionNumber { get; set; }


    public int ValidityYearBegin { get; set; }

    public int? ValidityYearFinish { get; set; }

    public MultiOwner(string? rut, double? ownershipPercentage, string commune, string block, string property, int? sheets, DateTime inscriptionDate, int? inscriptionNumber, int validityYearBegin, int? validityYearFinish)
    {
        Rut = rut;
        OwnershipPercentage = ownershipPercentage;
        Commune = commune;
        Block = block;
        Property = property;
        Sheets = sheets;
        InscriptionDate = inscriptionDate;
        InscriptionNumber = inscriptionNumber;
        ValidityYearBegin = validityYearBegin;
        ValidityYearFinish = validityYearFinish;
    }

}
