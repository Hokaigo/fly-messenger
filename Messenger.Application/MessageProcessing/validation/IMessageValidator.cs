namespace Messenger.Application.MessageProcessing.validation
{
    public interface IMessageValidator
    {
        Task ValidateAsync(string? text);
    }
}
