using System;
using System.Text;

namespace NLox.AST
{
    public static class AstPrinter
    {
        public static string Print(Expr expression) => expression switch
        {
            BinaryExpr b => Parenthesise(b.Operator.Lexeme, b.Left, b.Right),
            GroupingExpr g => Parenthesise("group", g.Expression),
            LiteralExpr l when l.Value is null => "nil",
            LiteralExpr l => l.Value.ToString(),
            UnaryExpr u => Parenthesise(u.Operator.Lexeme, u.Right),
            _ => throw new ArgumentException("Unknown expression type", nameof(expression))
        };

        private static string Parenthesise(string name, params Expr[] expressions)
        {
            var sb = new StringBuilder();

            sb.Append('(').Append(name);
            foreach (var expression in expressions)
            {
                sb.Append(' ').Append(Print(expression));
            }
            sb.Append(')');

            return sb.ToString();
        }
    }
}
