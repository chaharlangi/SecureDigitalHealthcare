using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;

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

    [ValidateNever]
    public string ProfileImagePath { get; set; } = null!;
}
