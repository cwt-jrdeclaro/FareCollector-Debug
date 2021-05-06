using System;

namespace FareCollector.Exceptions
{
    internal class FareRuleRetrievalException : Exception
    {
        public FareRuleRetrievalException()
        {
        }

        public FareRuleRetrievalException(string message)
        : base(message)
    {
        }

        public FareRuleRetrievalException(string message, Exception inner)
        : base(message, inner)
    {
        }
    }
}