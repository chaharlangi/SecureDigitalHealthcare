using System;
using System.Collections.Generic;

namespace SecureDigitalHealthcare.Models;

public partial class Speciality
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}
