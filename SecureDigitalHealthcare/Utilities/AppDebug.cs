namespace SecureDigitalHealthcare.Utilities
{
    public static class AppDebug
    {
        public static void Log(object message)
        {
            string finalMessage = $"AppDebug:   {message}";

            var length = finalMessage.ToString().Length;
            //for (int i = 0; i < length; i++)
            //{
            //    Console.Write($"-");
            //}
            Console.WriteLine();
            Console.WriteLine(finalMessage);
            for (int i = 0; i < length; i++)
            {
                Console.Write($"-");
            }
            Console.WriteLine();
        }
    }
}
