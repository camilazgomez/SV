using System;
using System.Collections.Generic;

namespace SV.Models;

public partial class RealStateForm
{
    public int AttentionNumber { get; set; }

    public string NatureOfTheDeed { get; set; } = null!;

    public string Commune { get; set; } = null!;

    public string Block { get; set; } = null!;

    public string Property { get; set; } = null!;

    public int Sheets { get; set; }

    public DateTime InscriptionDate { get; set; }

    public int InscriptionNumber { get; set; }

    public virtual ICollection<MultiOwner> MultiOwners { get; } = new List<MultiOwner>();

    public virtual ICollection<Person> People { get; } = new List<Person>();
}
