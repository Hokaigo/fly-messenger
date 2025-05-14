namespace Messenger.Application.MessageProcessing.validation
{
    public class InvalidMessageException : Exception
    {
        public InvalidMessageException(string message) : base(message) { }
    }

}
