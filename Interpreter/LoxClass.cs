using System.Collections.Generic;

namespace NLox
{
    public class LoxClass : ICallable
    {
        private string Name { get; }

        public LoxClass(string name)
        {
            Name = name;
        }

        public override string ToString() => Name;

        public int Arity() => 0;

        public object Call(Interpreter interpreter, List<object> arguments) => new LoxInstance(this);
    }
}