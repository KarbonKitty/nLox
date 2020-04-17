using System.Collections.Generic;
using NLox.AST;

namespace NLox
{
    public sealed class LoxFunction : ICallable
    {
        private FunctionStmt Declaration { get; }
        private Environment Closure { get; }

        public LoxFunction(FunctionStmt declaration, Environment closure)
        {
            Declaration = declaration;
            Closure = closure;
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
                return r.Value;
            }
            return null;
        }

        public override string ToString() => $"<fn {Declaration.Name.Lexeme}>";
    }
}
