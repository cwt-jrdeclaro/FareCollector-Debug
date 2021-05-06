using System;

namespace FareCollector.Exceptions
{
    internal class UnableToSendToManagerExeption : Exception
    {
        public UnableToSendToManagerExeption()
        {
        }

        public UnableToSendToManagerExeption(string message)
        : base(message)
        {
        }

        public UnableToSendToManagerExeption(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}