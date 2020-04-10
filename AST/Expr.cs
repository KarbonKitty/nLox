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

    public class VarExpr : Expr
    {
        public Token Name { get; }

        public VarExpr(Token name)
        {
            Name = name;
        }
    }

    public class AssignmentExpr : Expr
    {
        public Token Name { get; }
        public Expr Value { get; }

        public AssignmentExpr(Token name, Expr value)
        {
            Name = name;
            Value = value;
        }
    }

    public class LogicalExpr : Expr
    {
        public Expr Left { get; }
        public Expr Right { get; }
        public Token Operator { get; }

        public LogicalExpr(Expr left, Token op, Expr right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }
    }
}
