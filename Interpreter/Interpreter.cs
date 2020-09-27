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
        private readonly Dictionary<Expr, int> locals = new Dictionary<Expr, int>();

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

        public void Resolve(Expr expression, int depth)
        {
            locals.Add(expression, depth);
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
                else if (c.ElseBranch != null)
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
            if (statement is FunctionStmt f)
            {
                var fn = new LoxFunction(f, env);
                env.Define(f.Name.Lexeme, fn);
                return;
            }
            if (statement is ClassStmt cl)
            {
                env.Define(cl.Name.Lexeme, null);

                var methods = new Dictionary<string, LoxFunction>();
                foreach (var method in cl.Methods)
                {
                    var func = new LoxFunction(method, env);
                    methods.Add(method.Name.Lexeme, func);
                }

                var cls = new LoxClass(cl.Name.Lexeme, methods);
                env.Assign(cl.Name, cls);
                return;
            }
            if (statement is ReturnStmt r)
            {
                object value = null;
                if (r.Value != null)
                {
                    value = Evaluate(r.Value);
                }
                throw new ReturnException(value);
            }
            throw new RuntimeException("Unknown statement type.");
        }

        internal void ExecuteBlock(List<Stmt> statements, Environment environment)
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
            VarExpr v => LookUpVariable(v.Name, v),
            AssignmentExpr a => AssignVariable(a),
            LogicalExpr o => Logical(o),
            GetExpr get => GetProperty(get),
            SetExpr set => SetProperty(set),
            _ => throw new ArgumentException("Unknown expression type", nameof(expression))
        };

        private object SetProperty(SetExpr set)
        {
            var obj = Evaluate(set.Object);

            if (obj is not LoxInstance instance)
            {
                throw new RuntimeException(set.Name, "Only instances have fields.");
            }

            var val = Evaluate(set.Value);
            instance.Set(set.Name, val);
            return val;
        }

        private object GetProperty(GetExpr get)
        {
            var obj = Evaluate(get.Object);
            if (obj is LoxInstance instance)
            {
                return instance.Get(get.Name);
            }

            throw new RuntimeException(get.Name, "Only instances have properties.");
        }

        private object LookUpVariable(Token name, Expr expression)
        {
            var local = locals.TryGetValue(expression, out int distance);
            if (local)
            {
                return env.GetAt(distance, name.Lexeme);
            }
            else
            {
                return Globals.Get(name);
            }
        }

        private object AssignVariable(AssignmentExpr assignment)
        {
            var val = Evaluate(assignment.Value);
            var local = locals.TryGetValue(assignment, out int distance);
            if (local)
            {
                return env.AssignAt(distance, assignment.Name, val);
            }
            else
            {
                return Globals.Assign(assignment.Name, val);
            }
        }

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
