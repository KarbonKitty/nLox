using System;
using System.Collections.Generic;
using NLox.AST;
using NLox.Scanner;

namespace NLox
{
    public static class Interpreter
    {
        public static object Interpret(List<Stmt> statements)
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

        private static void Execute(Stmt statement)
        {
            if (statement is PrintStmt p)
            {
                Print(p.expression);
                return;
            }
            if (statement is ExpressionStmt e)
            {
                Discard(e.expression);
                return;
            }
            throw new RuntimeException("Unknown statement type.");
        }

        private static void Discard(Expr expression) => Evaluate(expression);

        private static void Print(Expr expression) => Console.WriteLine(Evaluate(expression));

        private static object Evaluate(Expr expression) => expression switch
        {
            BinaryExpr b => Binary(b),
            GroupingExpr g => Evaluate(g.Expression),
            LiteralExpr l => l.Value,
            UnaryExpr u => Unary(u),
            _ => throw new ArgumentException(nameof(expression))
        };

        private static object Unary(UnaryExpr unary)
        {
            object right = Evaluate(unary);

            static double TryNegate(Token token, object o)
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

        private static object Binary(BinaryExpr binary)
        {
            object left = Evaluate(binary.Left);
            object right = Evaluate(binary.Right);

            static TResult TryTypeAndOperate<TOperand, TResult>(Token token, object l, object r, Func<TOperand, TOperand, TResult> operation)
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

        private static bool IsEqual(object l, object r)
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

        private static bool IsTruthy(object o) => o switch
        {
            null => false,
            bool b => b,
            _ => true
        };

        private static T CheckTypeUnary<T>(Token token, object o)
        {
            if (o is T ot)
            {
                return ot;
            }

            throw new RuntimeException(token, $"Operand must be of type {typeof(T)}.");
        }

        private static (T, T) CheckTypeBinary<T>(Token token, object l, object r)
        {
            if (l is T lt && r is T rt)
            {
                return (lt, rt);
            }

            throw new RuntimeException(token, $"Both operands must be of type {typeof(T)}.");
        }
    }
}
