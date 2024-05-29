namespace SecureDigitalHealthcare.DTOs
{
    public record ChangePasswordDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string OTP { get; set; }
        public bool IsResetLink { get; set; }
    }
}
