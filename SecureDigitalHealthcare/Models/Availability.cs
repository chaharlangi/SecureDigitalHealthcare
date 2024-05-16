using System;
using System.Collections.Generic;

namespace SecureDigitalHealthcare.Models;

public partial class Availability
{
    public int Id { get; set; }

    public int DoctorId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool Taken { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual Doctor Doctor { get; set; } = null!;
}
