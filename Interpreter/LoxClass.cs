using System.Collections.Generic;

namespace NLox
{
    public class LoxClass : ICallable
    {
        public const string ConstructorName = "init";
        private string Name { get; }
        private LoxClass Superclass { get; }
        private Dictionary<string, LoxFunction> Methods { get; }

        public LoxClass(string name, LoxClass superclass, Dictionary<string, LoxFunction> methods)
        {
            Name = name;
            Superclass = superclass;
            Methods = methods;
        }

        public LoxFunction FindMethod(string name)
        {
            var hasMethod = Methods.TryGetValue(name, out var method);

            if (hasMethod)
            {
                return method;
            }
            else if (Superclass is not null)
            {
                return Superclass.FindMethod(name);
            }

            return null;
        }

        public int Arity()
        {
            var initializer = FindMethod(ConstructorName);
            return initializer is not null ? initializer.Arity() : 0;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            var instance = new LoxInstance(this);
            var initializer = FindMethod(ConstructorName);

            if (initializer is not null)
            {
                initializer.Bind(instance).Call(interpreter, arguments);
            }

            return instance;
        }

        public override string ToString() => Name;
    }
}
