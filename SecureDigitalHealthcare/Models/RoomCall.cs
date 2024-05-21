using System;
using System.Collections.Generic;

namespace SecureDigitalHealthcare.Models;

public partial class RoomCall
{
    public string Id { get; set; } = null!;

    public string? HostId { get; set; }

    public string? GuestId { get; set; }

    public string? HostAccessToken { get; set; }

    public string? GuestAccessToken { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
