namespace Messenger.Application.MessageProcessing.validation.validators
{
    public class SpamValidator : IMessageValidator
    {
        public Task ValidateAsync(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return Task.CompletedTask;

            var repeated = text.GroupBy(c => c).Any(g => g.Count() > 20);
            if (repeated)
                throw new InvalidMessageException("Message appears to be spam.");

            return Task.CompletedTask;
        }
    }
}
