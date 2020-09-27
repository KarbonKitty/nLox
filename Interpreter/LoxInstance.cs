using System.Collections.Generic;
using NLox.Scanner;

namespace NLox
{
    public class LoxInstance
    {
        private readonly LoxClass cls;
        private readonly Dictionary<string, object> fields = new Dictionary<string, object>();

        public LoxInstance(LoxClass cls)
        {
            this.cls = cls;
        }

        public object Get(Token name)
        {
            var hasValue = fields.TryGetValue(name.Lexeme, out var value);

            if (hasValue)
            {
                return value;
            }

            var (hasMethod, method) = cls.FindMethod(name.Lexeme);

            if (hasMethod)
            {
                return method;
            }

            throw new RuntimeException(name, $"Undefined property '{name.Lexeme}'.");
        }

        public void Set(Token name, object val)
        {
            fields[name.Lexeme] = val;
        }

        public override string ToString() => cls.ToString() + " instance";
    }
}
