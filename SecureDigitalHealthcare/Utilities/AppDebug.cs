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
        public static void Log(params object[] message)
        {
            string finalMessage = $"AppDebug:   ";

            var length = finalMessage.ToString().Length;

            Console.WriteLine();
            for (int i = 0; i < message.Length; i++)
            {
                object? item = message[i];
                Console.Write(item);
                if (i != message.Length - 1)
                {
                    Console.Write("\t");
                }
            }
            Console.WriteLine();
            for (int i = 0; i < length; i++)
            {
                Console.Write($"-");
            }
            Console.WriteLine();
        }
    }
}
