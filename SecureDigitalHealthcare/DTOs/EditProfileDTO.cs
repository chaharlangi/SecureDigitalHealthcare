namespace SecureDigitalHealthcare.DTOs
{
    public record EditProfileDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public DateTime BirthDate { get; set; }
        public string ProfileImagePath { get; set; }
    }
}