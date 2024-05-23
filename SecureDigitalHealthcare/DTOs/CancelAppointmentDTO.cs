namespace SecureDigitalHealthcare.DTOs
{
    public record CancelAppointmentDTO(int DoctorId, int PatientId, int AvailabilityId);
}
