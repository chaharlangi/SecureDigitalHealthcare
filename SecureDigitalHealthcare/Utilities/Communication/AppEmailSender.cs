using Azure.Communication.Email;
using Azure;

namespace SecureDigitalHealthcare.Utilities.Communication
{
    public class AppEmailSender : IAppSender
    {
        private const string ConnectionString = "endpoint=https://easyhealthcommunicationservice.europe.communication.azure.com/;accesskey=i3cI3sXbr/agwxQs1lvJTq9zMpwHVRT7x1TaOM3EghKXj3hspvDj6hMzy59MI9ap7omCkQLws2cZlTFVY1tBjw==";
        private const string SenderAddress = "DoNotReply@20795b72-9ffd-4896-bd62-2e6bebc46a7b.azurecomm.net";

        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public IAppSender Setup(string to, string subject, string body)
        {
            To = to;
            Subject = subject;
            Body = body;

            return this;
        }

        public async Task<bool> Send()
        {
            var emailClient = new EmailClient(ConnectionString);


            EmailSendOperation emailSendOperation = await emailClient.SendAsync(
                WaitUntil.Completed,
                senderAddress: SenderAddress,
                recipientAddress: $"{To}",
                subject: $"{Subject}",
                htmlContent: $"<html><h3>{Body}</h3l></html>",
                plainTextContent: $"{Body}");

            if (emailSendOperation.HasValue)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
