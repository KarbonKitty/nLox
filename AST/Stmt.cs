using NLox.Scanner;

namespace NLox.AST
{
    public abstract class Stmt { }

    public class ExpressionStmt : Stmt
    {
        public readonly Expr expression;

        public ExpressionStmt(Expr expression)
        {
            this.expression = expression;
        }
    }

    public class PrintStmt: Stmt
    {
        public readonly Expr expression;

        public PrintStmt(Expr expression)
        {
            this.expression = expression;
        }
    }

    public class VarStmt : Stmt
    {
        public readonly Token name;
        public readonly Expr initializer;

        public VarStmt(Token name, Expr initializer)
        {
            this.name = name;
            this.initializer = initializer;
        }
    }
}
