using System.Collections.Generic;

namespace NLox
{
    public abstract class NativeFunction : ICallable
    {
        public abstract int Arity();
        public abstract object Call(Interpreter interpreter, List<object> arguments);
    }

    public sealed class Clock : NativeFunction
    {
        public override int Arity() => 0;

        public override object Call(Interpreter interpreter, List<object> arguments)
            => (double)System.DateTime.Now.Ticks / System.TimeSpan.TicksPerSecond;

        public override string ToString() => "<native fn>";
    }
}
