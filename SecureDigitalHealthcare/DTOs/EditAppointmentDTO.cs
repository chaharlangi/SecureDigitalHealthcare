namespace SecureDigitalHealthcare.DTOs
{
    public record EditAppointmentDTO(int PatientId, int DoctorId, int AvailabilityId, string Symptom, string Disease, string DoctorDescription);
}
