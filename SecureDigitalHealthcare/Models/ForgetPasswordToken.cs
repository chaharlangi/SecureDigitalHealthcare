using System;
using System.Collections.Generic;

namespace SecureDigitalHealthcare.Models;

public partial class ForgetPasswordToken
{
    public int Id { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpirationDate { get; set; }

    public virtual User IdNavigation { get; set; } = null!;
}
