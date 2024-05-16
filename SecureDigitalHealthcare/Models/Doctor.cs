using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;

namespace SecureDigitalHealthcare.Models;

public partial class Doctor
{
    public int Id { get; set; }

    public int? SpecialityId { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<Availability> Availabilities { get; set; } = new List<Availability>();

    [ValidateNever]
    public virtual User IdNavigation { get; set; } = null!;

    public virtual Speciality? Speciality { get; set; }
}
