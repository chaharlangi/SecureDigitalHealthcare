using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;

namespace SecureDigitalHealthcare.Models;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;
    public string Address { get; set; } = null!;
    [ValidateNever]
    public string ProfileImagePath { get; set; } = null!;

    public DateTime BirthDate { get; set; }

    public DateTime RegistrationDate { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int? RoleId { get; set; }

    public bool AgreedTerms { get; set; }

    public bool Gender { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<Comment> CommentReceivers { get; set; } = new List<Comment>();

    public virtual ICollection<Comment> CommentSenders { get; set; } = new List<Comment>();

    public virtual Doctor? Doctor { get; set; }

    public virtual ForgetPasswordToken? ForgetPasswordToken { get; set; }

    public virtual Role? Role { get; set; }
}
