using NLox.Scanner;

namespace NLox.AST
{
    public abstract class Expr
    {
    }

    public class BinaryExpr : Expr
    {
        public Expr Left { get; }
        public Expr Right { get; }
        public Token Operator { get; }

        public BinaryExpr(Expr left, Token @operator, Expr right)
        {
            Left = left;
            Right = right;
            Operator = @operator;
        }
    }

    public class UnaryExpr : Expr
    {
        public Expr Right { get; }
        public Token Operator { get; }

        public UnaryExpr(Token @operator, Expr right)
        {
            Right = right;
            Operator = @operator;
        }
    }

    public class LiteralExpr : Expr
    {
        public object Value { get; }

        public LiteralExpr(object value)
        {
            Value = value;
        }
    }

    public class GroupingExpr : Expr
    {
        public Expr Expression { get; }

        public GroupingExpr(Expr expression)
        {
            Expression = expression;
        }
    }
}
