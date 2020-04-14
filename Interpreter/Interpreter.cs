using System;
using System.Collections.Generic;
using NLox.AST;
using NLox.Scanner;

namespace NLox
{
    public class Interpreter
    {
        public Environment Globals { get; }
        private Environment env;

        public Interpreter()
        {
            Globals = new Environment();
            env = Globals;

            Globals.Define("clock", new Clock());
        }

        public object Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeException rex)
            {
                Program.RuntimeError(rex);
            }
            return 0;
        }

        private void Execute(Stmt statement)
        {
            if (statement is PrintStmt p)
            {
                Print(p.Expression);
                return;
            }
            if (statement is VariableStmt s)
            {
                object value = s.Initializer is null ? null : Evaluate(s.Initializer);

                env.Define(s.Name.Lexeme, value);
                return;
            }
            if (statement is ExpressionStmt e)
            {
                Discard(e.Expression);
                return;
            }
            if (statement is BlockStmt b)
            {
                ExecuteBlock(b.Statements, new Environment(env));
                return;
            }
            if (statement is ConditionalStmt c)
            {
                if (IsTruthy(Evaluate(c.Condition)))
                {
                    Execute(c.ThenBranch);
                }
                else
                {
                    Execute(c.ElseBranch);
                }
                return;
            }
            if (statement is WhileStmt w)
            {
                while (IsTruthy(Evaluate(w.Condition)))
                {
                    Execute(w.Body);
                }
                return;
            }
            throw new RuntimeException("Unknown statement type.");
        }

        private void ExecuteBlock(List<Stmt> statements, Environment environment)
        {
            var previous = env;
            try
            {
                env = environment;
                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                env = previous;
            }
        }

        private void Discard(Expr expression) => Evaluate(expression);

        private void Print(Expr expression) => Console.WriteLine(Evaluate(expression));

        private object Evaluate(Expr expression) => expression switch
        {
            BinaryExpr b => Binary(b),
            CallExpr c => Call(c),
            GroupingExpr g => Evaluate(g.Expression),
            LiteralExpr l => l.Value,
            UnaryExpr u => Unary(u),
            VarExpr v => env.Get(v.Name),
            AssignmentExpr a => env.Assign(a.Name, Evaluate(a.Value)),
            LogicalExpr o => Logical(o),
            _ => throw new ArgumentException(nameof(expression))
        };

        private object Call(CallExpr call)
        {
            var callee = Evaluate(call.Callee);

            var arguments = new List<object>();
            foreach (var argument in call.Arguments)
            {
                arguments.Add(Evaluate(argument));
            }

            ICallable function = callee as ICallable
                ?? throw new RuntimeException(call.Paren, "Can only call functions and classes.");
            if (arguments.Count != function.Arity())
            {
                throw new RuntimeException(call.Paren,
                    $"Expected {function.Arity()} arguments but got {arguments.Count} instead.");
            }
            return function.Call(this, arguments);
        }

        private object Logical(LogicalExpr logical)
        {
            var left = Evaluate(logical.Left);

            if (logical.Operator.Type == TokenType.Or)
            {
                if (IsTruthy(left))
                {
                    return left;
                }
            }
            else if (!IsTruthy(left))
            {
                return left;
            }

            return Evaluate(logical.Right);
        }

        private object Unary(UnaryExpr unary)
        {
            object right = Evaluate(unary);

            double TryNegate(Token token, object o)
            {
                var d = CheckTypeUnary<double>(token, o);
                return -d;
            }

            return unary.Operator.Type switch
            {
                TokenType.Minus => TryNegate(unary.Operator, right),
                TokenType.Bang => !IsTruthy(right),
                _ => null
            };
        }

        private object Binary(BinaryExpr binary)
        {
            object left = Evaluate(binary.Left);
            object right = Evaluate(binary.Right);

            TResult TryTypeAndOperate<TOperand, TResult>(Token token, object l, object r, Func<TOperand, TOperand, TResult> operation)
            {
                var (left, right) = CheckTypeBinary<TOperand>(token, l, r);
                return operation(left, right);
            }

            return binary.Operator.Type switch
            {
                TokenType.Minus => TryTypeAndOperate<double, double>(binary.Operator, left, right, (l, r) => l - r),
                TokenType.Slash => TryTypeAndOperate<double, double>(binary.Operator, left, right, (l, r) => l / r),
                TokenType.Star => TryTypeAndOperate<double, double>(binary.Operator, left, right, (l, r) => l * r),
                TokenType.Plus => left is double ld && right is double rd
                    ? (object)(ld + rd)
                    : left is string ls && right is string rs ? ls + rs : throw new RuntimeException(binary.Operator, "Both operands must be either numbers or strings."),
                TokenType.Greater => TryTypeAndOperate<double, bool>(binary.Operator, left, right, (l, r) => l > r),
                TokenType.GreaterOrEqual => TryTypeAndOperate<double, bool>(binary.Operator, left, right, (l, r) => l >= r),
                TokenType.Less => TryTypeAndOperate<double, bool>(binary.Operator, left, right, (l, r) => l < r),
                TokenType.LessOrEqual => TryTypeAndOperate<double, bool>(binary.Operator, left, right, (l, r) => l <= r),
                TokenType.DoubleEqual => IsEqual(left, right),
                TokenType.BangEqual => !IsEqual(left, right),
                _ => null
            };
        }

        private bool IsEqual(object l, object r)
        {
            if (l is null && r is null)
            {
                return true;
            }
            if (l is null)
            {
                return false;
            }

            return l.Equals(r);
        }

        private bool IsTruthy(object o) => o switch
        {
            null => false,
            bool b => b,
            _ => true
        };

        private T CheckTypeUnary<T>(Token token, object o)
        {
            if (o is T ot)
            {
                return ot;
            }

            throw new RuntimeException(token, $"Operand must be of type {typeof(T)}.");
        }

        private (T, T) CheckTypeBinary<T>(Token token, object l, object r)
        {
            if (l is T lt && r is T rt)
            {
                return (lt, rt);
            }

            throw new RuntimeException(token, $"Both operands must be of type {typeof(T)}.");
        }
    }
}
