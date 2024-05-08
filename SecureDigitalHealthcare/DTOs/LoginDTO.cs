using System.ComponentModel.DataAnnotations;

namespace SecureDigitalHealthcare.DTOs;

public record LoginDTO([Required] string Email, [Required] string Password, [Required] bool RememberMe);
