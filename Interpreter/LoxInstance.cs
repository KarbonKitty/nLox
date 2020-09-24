namespace NLox
{
    public class LoxInstance
    {
        private readonly LoxClass cls;

        public LoxInstance(LoxClass cls)
        {
            this.cls = cls;
        }

        public override string ToString() => cls.ToString() + " instance";
    }
}
