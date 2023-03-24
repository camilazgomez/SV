using System;
using System.Collections.Generic;

namespace SV.Models;

public partial class Persona
{
    public int Id { get; set; }

    public string? Rut { get; set; }

    public string Nombre { get; set; } = null!;

    public DateTime FechaNacimiento { get; set; }

    public string Email { get; set; } = null!;

    public string? Dirección { get; set; }
}
