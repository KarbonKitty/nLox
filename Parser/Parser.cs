using System.Collections.Generic;
using NLox.AST;
using NLox.Scanner;

namespace NLox
{
    public class Parser
    {
        private readonly List<Token> tokens;
        private int current = 0;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public Expr Parse()
        {
            try
            {
                return Expression();
            }
            catch (ParseException)
            {
                return null;
            }
        }

        private Expr Expression()
        {
            return Equality();
        }

        private Expr Equality()
        {
            Expr expr = Comparison();

            while (Match(TokenType.BangEqual, TokenType.DoubleEqual))
            {
                var op = Previous();
                var right = Comparison();
                expr = new BinaryExpr(expr, op, right);
            }

            return expr;
        }

        private Expr Comparison()
        {
            var expr = Addition();

            while (Match(TokenType.Greater, TokenType.GreaterOrEqual, TokenType.Less, TokenType.LessOrEqual))
            {
                var op = Previous();
                var right = Addition();
                expr = new BinaryExpr(expr, op, right);
            }

            return expr;
        }

        private Expr Addition()
        {
            var expr = Multiplication();

            while (Match(TokenType.Minus, TokenType.Plus))
            {
                var op = Previous();
                var right = Multiplication();
                expr = new BinaryExpr(expr, op, right);
            }

            return expr;
        }

        private Expr Multiplication()
        {
            var expr = Unary();

            while (Match(TokenType.Slash, TokenType.Star))
            {
                var op = Previous();
                var right = Unary();
                expr = new BinaryExpr(expr, op, right);
            }

            return expr;
        }

        private Expr Unary()
        {
            if (Match(TokenType.Bang, TokenType.Minus))
            {
                var op = Previous();
                var right = Unary();
                return new UnaryExpr(op, right);
            }

            return Primary();
        }

        private Expr Primary()
        {
            if (Match(TokenType.False))
            {
                return new LiteralExpr(false);
            }
            if (Match(TokenType.True))
            {
                return new LiteralExpr(true);
            }
            if (Match(TokenType.Nil))
            {
                return new LiteralExpr(null);
            }

            if (Match(TokenType.Number, TokenType.String))
            {
                return new LiteralExpr(Previous().Literal);
            }

            if (Match(TokenType.LeftParen))
            {
                var expr = Expression();
                Consume(TokenType.RightParen, "Expected ')' after expression.");
                return new GroupingExpr(expr);
            }

            throw Error(Peek(), "Expect expression");
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type))
            {
                return Advance();
            }

            throw Error(Peek(), message);
        }

        private ParseException Error(Token token, string message)
        {
            Program.Error(token, message);
            return new ParseException(message);
        }

        private bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private bool Check(TokenType type) => !IsAtEnd() && Peek().Type == type;

        private Token Advance()
        {
            current += IsAtEnd() ? 0 : 1;

            return Previous();
        }

        private bool IsAtEnd() => Peek().Type == TokenType.EOF;

        private Token Previous() => tokens[current - 1];

        private Token Peek() => tokens[current];

        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if (Previous().Type == TokenType.Semicolon)
                {
                    return;
                }

                switch (Peek().Type)
                {
                    case TokenType.Class:
                    case TokenType.Fun:
                    case TokenType.Var:
                    case TokenType.For:
                    case TokenType.If:
                    case TokenType.While:
                    case TokenType.Print:
                    case TokenType.Return:
                        return;
                }
            }

            Advance();
        }
    }
}