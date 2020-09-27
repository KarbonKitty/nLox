using System.Collections.Generic;

namespace NLox
{
    public class LoxClass : ICallable
    {
        private string Name { get; }
        private Dictionary<string, LoxFunction> Methods { get; }

        public LoxClass(string name, Dictionary<string, LoxFunction> methods)
        {
            Name = name;
            Methods = methods;
        }

        public (bool, LoxFunction) FindMethod(string name)
        {
            var hasMethod = Methods.TryGetValue(name, out var method);
            return (hasMethod, method);
        }

        public int Arity() => 0;

        public object Call(Interpreter interpreter, List<object> arguments) => new LoxInstance(this);

        public override string ToString() => Name;
    }
}