using System.Collections.Generic;
using NLox.AST;

namespace NLox
{
    public sealed class LoxFunction : ICallable
    {
        private bool IsInitializer { get; }
        private FunctionStmt Declaration { get; }
        private Environment Closure { get; }

        public LoxFunction(FunctionStmt declaration, Environment closure, bool isInitializer)
        {
            Declaration = declaration;
            Closure = closure;
            IsInitializer = isInitializer;
        }

        public int Arity() => Declaration.Params.Count;

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            var env = new Environment(Closure);

            for (var i = 0; i < Declaration.Params.Count; i++)
            {
                env.Define(Declaration.Params[i].Lexeme, arguments[i]);
            }

            try
            {
                interpreter.ExecuteBlock(Declaration.Body, env);
            }
            catch (ReturnException r)
            {
                if (IsInitializer)
                {
                    return Closure.GetAt(0, "this");
                }
                return r.Value;
            }

            if (IsInitializer)
            {
                return Closure.GetAt(0, "this");
            }

            return null;
        }

        public LoxFunction Bind(LoxInstance instance)
        {
            var env = new Environment(Closure);
            env.Define("this", instance);
            return new LoxFunction(Declaration, env, IsInitializer);
        }

        public override string ToString() => $"<fn {Declaration.Name.Lexeme}>";
    }
}
