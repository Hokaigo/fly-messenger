namespace Messenger.Application.MessageProcessing.validation.validators
{
    public class ProfanityValidator : IMessageValidator
    {
        private static readonly string[] BannedWords = { "fuck", "rape", "suicide", "asshole" };

        public Task ValidateAsync(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return Task.CompletedTask;

            foreach (var word in BannedWords)
            {
                if (text.Contains(word, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidMessageException("Message contains inappropriate language.");
            }

            return Task.CompletedTask;
        }
    }
}
