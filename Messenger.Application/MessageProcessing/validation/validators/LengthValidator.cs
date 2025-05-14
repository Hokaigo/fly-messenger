namespace Messenger.Application.MessageProcessing.validation.validators
{
    public class LengthValidator : IMessageValidator
    {
        private const int MaxLength = 1000;

        public Task ValidateAsync(string? text)
        {
            if (text != null && text.Length > MaxLength)
                throw new InvalidMessageException($"Message exceeds max length ({MaxLength} characters).");
            return Task.CompletedTask;
        }
    }
}
