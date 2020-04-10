using System.Collections.Generic;
using NLox.Scanner;

namespace NLox.AST
{
    public abstract class Stmt { }

    public class ExpressionStmt : Stmt
    {
        public readonly Expr Expression;

        public ExpressionStmt(Expr expression)
        {
            Expression = expression;
        }
    }

    public class PrintStmt: Stmt
    {
        public readonly Expr Expression;

        public PrintStmt(Expr expression)
        {
            Expression = expression;
        }
    }

    public class VariableStmt : Stmt
    {
        public readonly Token Name;
        public readonly Expr Initializer;

        public VariableStmt(Token name, Expr initializer)
        {
            Name = name;
            Initializer = initializer;
        }
    }

    public class BlockStmt : Stmt
    {
        public readonly List<Stmt> Statements;

        public BlockStmt(List<Stmt> statements)
        {
            Statements = statements;
        }
    }

    public class ConditionalStmt : Stmt
    {
        public readonly Expr Condition;
        public readonly Stmt ThenBranch;
        public readonly Stmt ElseBranch;

        public ConditionalStmt(Expr condition, Stmt thenBranch, Stmt elseBranch = null)
        {
            Condition = condition;
            ThenBranch = thenBranch;
            ElseBranch = elseBranch;
        }
    }
}
