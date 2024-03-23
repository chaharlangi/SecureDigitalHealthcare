using System;
using System.Collections.Generic;

namespace SecureDigitalHealthcare.Models;

public partial class Student
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateTime BirthDate { get; set; }
}
