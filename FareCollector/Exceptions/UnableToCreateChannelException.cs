using System;

namespace FareCollector.Exceptions
{
    internal class UnableToCreateChannelException : Exception
    {
        public UnableToCreateChannelException()
        {
        }

        public UnableToCreateChannelException(string message)
        : base(message)
        {
        }

        public UnableToCreateChannelException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}