using System;

namespace NLox
{
    public sealed class ReturnException : Exception
    {
        public object Value { get; }

        public ReturnException(object value) => Value = value;

        public ReturnException()
        {
        }

        public ReturnException(string message) : base(message)
        {
        }

        public ReturnException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
