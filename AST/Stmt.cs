using System.Collections.Generic;
using NLox.Scanner;

namespace NLox.AST
{
    public abstract class Stmt { }

    public class ExpressionStmt : Stmt
    {
        public Expr Expression { get; }

        public ExpressionStmt(Expr expression)
        {
            Expression = expression;
        }
    }

    public class PrintStmt: Stmt
    {
        public Expr Expression { get; }

        public PrintStmt(Expr expression)
        {
            Expression = expression;
        }
    }

    public class VariableStmt : Stmt
    {
        public Token Name { get; }
        public Expr Initializer { get; }

        public VariableStmt(Token name, Expr initializer)
        {
            Name = name;
            Initializer = initializer;
        }
    }

    public class BlockStmt : Stmt
    {
        public List<Stmt> Statements { get; }

        public BlockStmt(List<Stmt> statements)
        {
            Statements = statements;
        }
    }

    public class ConditionalStmt : Stmt
    {
        public Expr Condition { get; }
        public Stmt ThenBranch { get; }
        public Stmt ElseBranch { get; }

        public ConditionalStmt(Expr condition, Stmt thenBranch, Stmt elseBranch = null)
        {
            Condition = condition;
            ThenBranch = thenBranch;
            ElseBranch = elseBranch;
        }
    }

    public class WhileStmt : Stmt
    {
        public Expr Condition { get; }
        public Stmt Body { get; }

        public WhileStmt(Expr condition, Stmt body)
        {
            Condition = condition;
            Body = body;
        }
    }

    public class FunctionStmt : Stmt
    {
        public Token Name { get; }
        public List<Token> Params { get; }
        public List<Stmt> Body { get; }

        public FunctionStmt(Token name, List<Token> parameters, List<Stmt> body)
        {
            Name = name;
            Params = parameters;
            Body = body;
        }
    }

    public class ReturnStmt : Stmt
    {
        public Token Keyword { get; }
        public Expr Value { get; }

        public ReturnStmt(Token keyword, Expr value)
        {
            Keyword = keyword;
            Value = value;
        }
    }
}
