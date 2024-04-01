using System;
using System.Collections.Generic;

namespace SecureDigitalHealthcare.Models;

public partial class User
{
    public string NationalId { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Gender { get; set; } = null!;

    public DateTime Birthdate { get; set; }

    public string Address { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public bool AgreeTerms { get; set; }
}
