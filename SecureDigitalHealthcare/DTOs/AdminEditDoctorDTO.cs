namespace SecureDigitalHealthcare.DTOs
{
    public record AdminEditDoctorDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int SpecialityId { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string ProfileImagePath { get; set; }
        public DateTime Birthdate { get; set; }
    }
}
