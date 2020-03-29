using System;
using System.Collections.Generic;

namespace NLox
{
    public class Scanner
    {
        private Dictionary<string, TokenType?> Keywords => new Dictionary<string, TokenType?>
        {
            { "and", TokenType.And },
            { "class", TokenType.Class },
            { "else", TokenType.Else },
            { "false", TokenType.False },
            { "for", TokenType.For },
            { "fun", TokenType.Fun },
            { "if", TokenType.If },
            { "nil", TokenType.Nil },
            { "or", TokenType.Or },
            { "print", TokenType.Print },
            { "return", TokenType.Return },
            { "super", TokenType.Super },
            { "this", TokenType.This },
            { "true", TokenType.True },
            { "var", TokenType.Var },
            { "while", TokenType.While }
        };

        private readonly string source;
        private readonly List<Token> tokens = new List<Token>();
        private int start = 0;
        private int current = 0;
        private int line = 1;

        public Scanner(string source)
        {
            this.source = source;
        }

        internal List<Token> ScanTokens()
        {
            while (current < source.Length)
            {
                start = current;
                ScanToken();
            }

            tokens.Add(new Token(TokenType.EOF, string.Empty, null, line));
            return tokens;
        }
        private void ScanToken()
        {
            char c = Advance();
            try
            {
                TokenType? tokenType = c switch
                {
                    '(' => TokenType.LeftParen,
                    ')' => TokenType.RightParen,
                    '{' => TokenType.LeftBrace,
                    '}' => TokenType.RightBrace,
                    ',' => TokenType.Comma,
                    '.' => TokenType.Dot,
                    '-' => TokenType.Minus,
                    '+' => TokenType.Plus,
                    ';' => TokenType.Semicolon,
                    '*' => TokenType.Star,
                    '!' => Match('=') ? TokenType.BangEqual : TokenType.Bang,
                    '=' => Match('=') ? TokenType.DoubleEqual : TokenType.Equal,
                    '<' => Match('=') ? TokenType.LessOrEqual : TokenType.Less,
                    '>' => Match('=') ? TokenType.GreaterOrEqual : TokenType.Greater,
                    '/' => Match('/') ? Comment() : TokenType.Slash,
                    ' ' => null,
                    '\r' => null,
                    '\t' => null,
                    '\n' => NewLine(),
                    '"' => String(),
                    var d when IsDigit(d) => Number(),
                    var a when IsAlpha(a) => Identifier(),
                    _ => throw new Exception("Unexpected character.")
                };

                if (tokenType != null)
                {
                    AddToken(tokenType.Value);
                }
            }
            catch (Exception ex)
            {
                Program.Error(line, ex.Message);
            }
        }

        private bool IsDigit(char c) => c >= '0' && c <= '9';
        private bool IsAlpha(char c) => c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c == '_';
        private bool IsAlphaNumeric(char c) => IsDigit(c) || IsAlpha(c);

        private TokenType? Identifier()
        {
            while (IsAlphaNumeric(Peek()))
            {
                Advance();
            }

            var text = source[start..current];
            TokenType type = Keywords.GetValueOrDefault(text) ?? TokenType.Identifier;

            AddToken(type);

            return null;
        }

        private TokenType? Number()
        {
            while (IsDigit(Peek()))
            {
                Advance();
            }

            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                // consume the '.'
                Advance();

                while (IsDigit(Peek()))
                {
                    Advance();
                }
            }

            AddToken(TokenType.Number, double.Parse(source[start..current]));

            return null;
        }

        private TokenType? String()
        {
            while (Peek() != '"' && current < source.Length)
            {
                if (Peek() == '\n')
                {
                    line++;
                }
                Advance();
            }

            // unterminated string
            if (current >= source.Length)
            {
                Program.Error(line, "Unterminated string");
            }

            // the closing "
            Advance();

            // trim the quotes
            var value = source[(start + 1)..(current - 1)];
            AddToken(TokenType.String, value);
            return null;
        }

        private bool Match(char expected)
        {
            if (current >= source.Length)
            {
                return false;
            }
            if (source[current] != expected)
            {
                return false;
            }

            current++;
            return true;
        }

        private TokenType? NewLine()
        {
            line++;
            return null;
        }

        private TokenType? Comment()
        {
            while (Peek() != '\n' && current < source.Length)
            {
                Advance();
            }
            return null;
        }

        private char Peek() => current >= source.Length ? '\0' : source[current];
        private char PeekNext() => current + 1 >= source.Length ? '\0' : source[current + 1];

        private char Advance() => source[current++];

        private void AddToken(TokenType type, object literal = null)
        {
            string text = source[start..current];
            tokens.Add(new Token(type, text, literal, line));
        }
    }
}
