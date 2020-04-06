using System.Collections.Generic;
using NLox.Scanner;

namespace NLox
{
    public class Environment
    {
        private readonly Dictionary<string, object> values = new Dictionary<string, object>();

        public void Define(string name, object value) => values.Add(name, value);

        public object Get(Token name) => values.ContainsKey(name.Lexeme)
            ? values[name.Lexeme]
            : throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}'.");

        public object Assign(Token name, object value)
        {
            if (values.ContainsKey(name.Lexeme))
            {
                values[name.Lexeme] = value;
                return value;
            }

            throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
        }
    }
}
