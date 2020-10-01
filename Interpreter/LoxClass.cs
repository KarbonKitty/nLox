using System.Collections.Generic;

namespace NLox
{
    public class LoxClass : ICallable
    {
        public const string ConstructorName = "init";
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

        public int Arity()
        {
            var (hasInitialier, initializer) = FindMethod(ConstructorName);
            return hasInitialier ? initializer.Arity() : 0;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            var instance = new LoxInstance(this);
            var (hasInitializer, initializer) = FindMethod(ConstructorName);

            if (hasInitializer)
            {
                initializer.Bind(instance).Call(interpreter, arguments);
            }

            return instance;
        }

        public override string ToString() => Name;
    }
}
