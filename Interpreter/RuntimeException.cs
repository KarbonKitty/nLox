using System;
using NLox.Scanner;

namespace NLox
{
    [Serializable]
    public class RuntimeException : Exception
    {
        public readonly Token token;

        public RuntimeException() { }
        public RuntimeException(string message) : base(message) { }
        public RuntimeException(string message, Exception inner) : base(message, inner) { }
        protected RuntimeException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        public RuntimeException(Token token, string message) : base(message)
        {
            this.token = token;
        }
    }
}
