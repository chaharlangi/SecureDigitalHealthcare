namespace SecureDigitalHealthcare.DTOs
{
    public record AvailabilityDTO
    {
        public int DoctorId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
