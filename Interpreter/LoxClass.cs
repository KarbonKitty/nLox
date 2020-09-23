namespace NLox
{
    public class LoxClass
    {
        private string Name { get; }

        public LoxClass(string name)
        {
            Name = name;
        }

        public override string ToString() => Name;
    }
}