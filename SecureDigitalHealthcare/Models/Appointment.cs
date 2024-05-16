using System;
using System.Collections.Generic;

namespace SecureDigitalHealthcare.Models;

public partial class Appointment
{
    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public int AvailabilityId { get; set; }

    public bool? Accepted { get; set; }

    public bool? Done { get; set; }

    public string? Symptom { get; set; }

    public string? Disease { get; set; }

    public string? DoctorDescription { get; set; }

    public virtual Availability Availability { get; set; } = null!;

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual User Patient { get; set; } = null!;
}
