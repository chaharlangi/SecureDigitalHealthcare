namespace SecureDigitalHealthcare.DTOs
{
    public record PredefinedAvailability(DateTime date, DateTime from, DateTime to, int minutes);
}
