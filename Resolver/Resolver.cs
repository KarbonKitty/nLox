using System.Collections.Generic;
using NLox.AST;
using NLox.Scanner;

namespace NLox
{
    public class Resolver
    {
        private readonly Interpreter interpreter;
        private readonly Stack<Dictionary<string, bool>> scopes;
        private FunctionType currentFunction = FunctionType.None;
        private ClassType currentClass = ClassType.None;

        public Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
            scopes = new Stack<Dictionary<string, bool>>();
        }

        public void Resolve(List<Stmt> statements)
        {
            foreach (var statement in statements)
            {
                Resolve(statement);
            }
        }

        private void Resolve(Stmt statement)
        {
            if (statement is BlockStmt block)
            {
                BeginScope();
                Resolve(block.Statements);
                EndScope();
            }
            else if (statement is VariableStmt variable)
            {
                Declare(variable.Name);
                if (variable.Initializer != null)
                {
                    Resolve(variable.Initializer);
                }
                Define(variable.Name);
            }
            else if (statement is FunctionStmt functionStatement)
            {
                Declare(functionStatement.Name);
                Define(functionStatement.Name);

                ResolveFunction(functionStatement, FunctionType.Function);
            }
            else if (statement is ConditionalStmt conditional)
            {
                Resolve(conditional.Condition);
                Resolve(conditional.ThenBranch);
                if (conditional.ElseBranch != null)
                {
                    Resolve(conditional.ElseBranch);
                }
            }
            else if (statement is ReturnStmt returnStmt)
            {
                if (currentFunction == FunctionType.None)
                {
                    Program.Error(returnStmt.Keyword, "Cannot return from top-level code.");
                }

                if (returnStmt.Value != null)
                {
                    if (currentFunction == FunctionType.Initializer)
                    {
                        Program.Error(returnStmt.Keyword, "Cannot return a value from an initializer.");
                    }
                    Resolve(returnStmt.Value);
                }
            }
            else if (statement is WhileStmt whileStmt)
            {
                Resolve(whileStmt.Condition);
                Resolve(whileStmt.Body);
            }
            else if (statement is ExpressionStmt expression)
            {
                Resolve(expression.Expression);
            }
            else if (statement is PrintStmt print)
            {
                Resolve(print.Expression);
            }
            else if (statement is ClassStmt cls)
            {
                var enclosingClass = currentClass;
                currentClass = ClassType.Class;

                Declare(cls.Name);
                Define(cls.Name);

                if (cls.Superclass != null && cls.Name.Lexeme.Equals(cls.Superclass.Name.Lexeme))
                {
                    Program.Error(cls.Superclass.Name, "A class cannot inherit from itself.");
                }

                if (cls.Superclass != null)
                {
                    currentClass = ClassType.Subclass;
                    Resolve(cls.Superclass);
                }

                if (cls.Superclass != null)
                {
                    BeginScope();
                    scopes.Peek()["super"] = true;
                }

                BeginScope();
                scopes.Peek().Add("this", true);

                foreach (var method in cls.Methods)
                {
                    var declaration = method.Name.Lexeme.Equals(LoxClass.ConstructorName) ? FunctionType.Initializer : FunctionType.Method;
                    ResolveFunction(method, declaration);
                }

                EndScope();

                if (cls.Superclass != null)
                {
                    EndScope();
                }

                currentClass = enclosingClass;
            }
            else
            {
                // After print statment was forgotten
                throw new System.Exception("You have forgotten something, haven't you?");
            }
        }

        private void Resolve(Expr expression)
        {
            if (expression is VarExpr variable)
            {
                if (scopes.TryPeek(out var scope) && scope.TryGetValue(variable.Name.Lexeme, out var value) && !value)
                {
                    Program.Error(variable.Name, "Cannot read local variable in its own initializer.");
                }

                ResolveLocal(variable, variable.Name);
            }
            else if (expression is AssignmentExpr assignment)
            {
                Resolve(assignment.Value);
                ResolveLocal(assignment, assignment.Name);
            }
            else if (expression is BinaryExpr binary)
            {
                Resolve(binary.Left);
                Resolve(binary.Right);
            }
            else if (expression is CallExpr call)
            {
                Resolve(call.Callee);
                foreach (var argument in call.Arguments)
                {
                    Resolve(argument);
                }
            }
            else if (expression is GroupingExpr grouping)
            {
                Resolve(grouping.Expression);
            }
            else if (expression is LogicalExpr logical)
            {
                Resolve(logical.Left);
                Resolve(logical.Right);
            }
            else if (expression is UnaryExpr unary)
            {
                Resolve(unary.Right);
            }
            else if (expression is GetExpr get)
            {
                Resolve(get.Object);
            }
            else if (expression is SetExpr set)
            {
                Resolve(set.Value);
                Resolve(set.Object);
            }
            else if (expression is SuperExpr sup)
            {
                if (currentClass == ClassType.None)
                {
                    Program.Error(sup.Keyword, "Cannot use 'super' outside of a class.");
                }
                else if (currentClass != ClassType.Subclass)
                {
                    Program.Error(sup.Keyword, "Cannot use 'super' in a class with no superclass.");
                }
                ResolveLocal(sup, sup.Keyword);
            }
            else if (expression is ThisExpr ths)
            {
                if (currentClass == ClassType.None)
                {
                    Program.Error(ths.Keyword, "Cannot use 'this' outside of a class.");
                    return;
                }
                ResolveLocal(ths, ths.Keyword);
            }
        }

        private void ResolveFunction(FunctionStmt function, FunctionType type)
        {
            var enclosingFunction = currentFunction;
            currentFunction = type;

            BeginScope();
            foreach (var param in function.Params)
            {
                Declare(param);
                Define(param);
            }
            Resolve(function.Body);
            EndScope();
            currentFunction = enclosingFunction;
        }

        private void BeginScope()
        {
            scopes.Push(new Dictionary<string, bool>());
        }

        private void EndScope()
        {
            scopes.Pop();
        }

        private void Declare(Token name)
        {
            if (scopes.TryPeek(out var scope))
            {
                if (scope.ContainsKey(name.Lexeme))
                {
                    Program.Error(name, "Variable with this name already declared in this scope.");
                }
                scope.Add(name.Lexeme, false);
            }
        }

        private void Define(Token name)
        {
            if (!scopes.TryPeek(out var scope))
            {
                return;
            }
            scope[name.Lexeme] = true;
        }

        private void ResolveLocal(Expr expression, Token name)
        {
            var i = scopes.Count - 1;
            foreach (var scope in scopes)
            {
                if (scope.ContainsKey(name.Lexeme))
                {
                    interpreter.Resolve(expression, scopes.Count - 1 - i);
                    return;
                }
                i--;
            }

            // not found in local scopes; assume global
        }
    }
}
