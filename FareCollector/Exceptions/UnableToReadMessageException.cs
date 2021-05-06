using System;

namespace FareCollector.Exceptions
{
    internal class UnableToReadMessageException : Exception
    {
        public string UnreadableMessage; 
        public UnableToReadMessageException()
        {
        }

        public UnableToReadMessageException(string message, string _unreadableMessage)
        : base(message)
        {
            UnreadableMessage = _unreadableMessage;
        }

        public UnableToReadMessageException(string message, string _unreadableMessage, Exception inner)
        : base(message, inner)
        {
            UnreadableMessage = _unreadableMessage;
        }
    }
}