namespace SecureDigitalHealthcare.Utilities
{
    public static class AppDebug
    {
        public static void Log(object message)
        {
            string finalMessage = $"AppDebug:   ";

            var length = finalMessage.ToString().Length;

            Console.WriteLine();
            Console.WriteLine(message);
            for (int i = 0; i < length; i++)
            {
                Console.Write($"-");
            }
            Console.WriteLine();
        }
    }
}
