namespace SecureDigitalHealthcare.DTOs
{
    public record RoomCallDTO()
    {
        public string? RoomId { get; init; }
        public string? UserId { get; init; }
        public string? AccessToken { get; init; }

        public override string ToString()
        {
            return $"RoomId: {RoomId}" +
                   $"\nHostId: {UserId}" +
                   $"\nHostAccessToken: {AccessToken}";
        }
    }
}
