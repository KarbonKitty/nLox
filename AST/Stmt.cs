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
}
