namespace NLox.Scanner
{
    public enum TokenType
    {
        // single-character tokens
        LeftParen,
        RightParen,
        LeftBrace,
        RightBrace,
        Comma,
        Dot,
        Minus,
        Plus,
        Semicolon,
        Slash,
        Star,

        // single- or double- character tokens
        Bang,
        BangEqual,
        Equal,
        DoubleEqual,
        Greater,
        GreaterOrEqual,
        Less,
        LessOrEqual,

        // identifiers
        Identifier,
        String,
        Number,

        // keywords
        And,
        Class,
        Else,
        False,
        Fun,
        For,
        If,
        Nil,
        Or,
        Print,
        Return,
        Super,
        This,
        True,
        Var,
        While,

        EOF
    }
}
