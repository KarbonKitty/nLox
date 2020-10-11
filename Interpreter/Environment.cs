using System.Collections.Generic;
using NLox.Scanner;

namespace NLox
{
    public class Environment
    {
        private readonly Dictionary<string, object> values = new Dictionary<string, object>();
        public Environment Enclosing { get; }

        public Environment()
        {
            this.Enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            this.Enclosing = enclosing;
        }

        public void Define(string name, object value) => values[name] = value;

        public object Get(Token name)
        {
            if (values.ContainsKey(name.Lexeme))
            {
                return values[name.Lexeme];
            }
            else if (Enclosing != null)
            {
                return Enclosing.Get(name);
            }

            throw new RuntimeException(name, $"Reading undefined variable '{name.Lexeme}'.");
        }

        public object GetAt(int distance, string name)
        {
            return Ancestor(distance).values[name];
        }

        public object Assign(Token name, object value)
        {
            if (values.ContainsKey(name.Lexeme))
            {
                values[name.Lexeme] = value;
                return value;
            }

            if (Enclosing != null)
            {
                return Enclosing.Assign(name, value);
            }

            throw new RuntimeException(name, $"Assigning to undefined variable '{name.Lexeme}'.");
        }

        public object AssignAt(int distance, Token name, object value)
        {
            Ancestor(distance).values[name.Lexeme] = value;
            return value;
        }

        private Environment Ancestor(int distance)
        {
            var environment = this;
            for (int i = 0; i < distance; i++)
            {
                environment = environment.Enclosing;
            }

            return environment;
        }
    }
}
