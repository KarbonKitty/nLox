using System.Collections.Generic;
using NLox.Scanner;

namespace NLox
{
    public class Environment
    {
        private readonly Dictionary<string, object> values = new Dictionary<string, object>();
        private readonly Environment enclosing;

        public Environment()
        {
            this.enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            this.enclosing = enclosing;
        }

        public void Define(string name, object value) => values[name] = value;

        public object Get(Token name)
        {
            if (values.ContainsKey(name.Lexeme))
            {
                return values[name.Lexeme];
            }
            else if (enclosing != null)
            {
                return enclosing.Get(name);
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

            if (enclosing != null)
            {
                return enclosing.Assign(name, value);
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
                environment = environment.enclosing;
            }

            return environment;
        }
    }
}
