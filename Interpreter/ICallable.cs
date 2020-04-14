using System.Collections.Generic;

namespace NLox
{
    public interface ICallable
    {
        int Arity();
        object Call(Interpreter interpreter, List<object> arguments);
    }
}
