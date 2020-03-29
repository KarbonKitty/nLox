namespace NLox
{
    internal class Token
    {
        internal TokenType Type { get; }
        internal string Lexeme { get; }
        internal object Literal { get; }
        internal int Line { get; }

        internal Token(TokenType type, string lexeme, object literal, int line)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            Line = line;
        }

        public override string ToString() => $"{Type} {Lexeme} {Literal}";
    }
}
