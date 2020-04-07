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
}
