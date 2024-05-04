using System;
using System.Collections.Generic;

namespace SecureDigitalHealthcare.Models;

public partial class Availability
{
    public int DoctorId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool Taken { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;
}
