
namespace SecureDigitalHealthcare.Utilities.Communication;

public interface IAppSender
{
    public enum MessageType
    {
        TFA,
        None
    }

    public string To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }

    public IAppSender Setup(string to, string subject, string body);

    public Task<bool> Send();

}
