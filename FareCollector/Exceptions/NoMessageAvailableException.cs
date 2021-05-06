using System;

namespace FareCollector.Exceptions
{
    internal class NoMessageAvailableException : Exception
    {
        public NoMessageAvailableException()
        {
        }

        public NoMessageAvailableException(string message)
        : base(message)
        {
        }

        public NoMessageAvailableException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}