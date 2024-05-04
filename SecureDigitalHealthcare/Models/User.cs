using System;
using System.Collections.Generic;

namespace SecureDigitalHealthcare.Models;

public partial class User
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? LastName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public string? ProfileImagePath { get; set; }

    public DateTime? BirthDate { get; set; }

    public DateTime? RegistrationDate { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public int RoleId { get; set; }

    public bool? AgreedTerms { get; set; }

    public bool? Gender { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<Comment> CommentReceivers { get; set; } = new List<Comment>();

    public virtual ICollection<Comment> CommentSenders { get; set; } = new List<Comment>();

    public virtual Doctor? Doctor { get; set; }

    public virtual Role Role { get; set; } = null!;
}
