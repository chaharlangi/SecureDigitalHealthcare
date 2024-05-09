namespace SecureDigitalHealthcare.Utilities
{
    public class BaseConverter
    {
        public static string Base64Encode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        public static byte[] Base64Decode(string input)
        {
            return Convert.FromBase64String(input);
        }
    }
}
