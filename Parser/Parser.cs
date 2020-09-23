using System.Collections.Generic;
using NLox.AST;
using NLox.Scanner;

namespace NLox
{
    public class Parser
    {
        private readonly List<Token> tokens;
        private int current = 0;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public List<Stmt> Parse()
        {
            var statements = new List<Stmt>();

            while (!IsAtEnd())
            {
                statements.Add(Declaration());
            }

            return statements;
        }

        private Stmt Declaration()
        {
            try
            {
                if (Match(TokenType.Class))
                {
                    return ClassDeclaration();
                }
                if (Match(TokenType.Fun))
                {
                    return Function("function");
                }
                if (Match(TokenType.Var))
                {
                    return VarDeclaration();
                }
                return Statement();
            }
            catch (ParseException)
            {
                Synchronize();
                return null;
            }
        }

        private FunctionStmt Function(string kind)
        {
            var name = Consume(TokenType.Identifier, $"Expect {kind} name.");
            Consume(TokenType.LeftParen, $"Expect '(' after {kind} name.");

            var parameters = new List<Token>();
            if (!Check(TokenType.RightParen))
            {
                do
                {
                    if (parameters.Count >= 255)
                    {
                        Error(Peek(), $"Cannot have more than 255 {kind} parameters.");
                    }

                    parameters.Add(Consume(TokenType.Identifier, "Expect parameter name."));
                } while (Match(TokenType.Comma));
            }

            Consume(TokenType.RightParen, $"Expect ')' after {kind} parameters.");

            Consume(TokenType.LeftBrace, $"Expect '{{' before {kind} body.");
            var body = Block();

            return new FunctionStmt(name, parameters, body);
        }

        private VariableStmt VarDeclaration()
        {
            var name = Consume(TokenType.Identifier, "Expect variable name.");

            Expr initializer = null;
            if (Match(TokenType.Equal))
            {
                initializer = Expression();
            }

            Consume(TokenType.Semicolon, "Expect ';' after variable declaration.");
            return new VariableStmt(name, initializer);
        }

        private ClassStmt ClassDeclaration()
        {
            var name = Consume(TokenType.Identifier, "Expect class name");
            Consume(TokenType.LeftBrace, "Expect '{' before class body.");

            var methods = new List<FunctionStmt>();
            while (!Check(TokenType.RightBrace) && !IsAtEnd())
            {
                methods.Add(Function("method"));
            }

            Consume(TokenType.RightBrace, "Expect '}' after class body.");

            return new ClassStmt(name, methods);
        }

        private Stmt Statement()
        {
            if (Match(TokenType.For))
            {
                return ForStatement();
            }
            if (Match(TokenType.If))
            {
                return ConditionalStatement();
            }
            if (Match(TokenType.Print))
            {
                return PrintStatement();
            }
            if (Match(TokenType.Return))
            {
                return ReturnStatement();
            }
            if (Match(TokenType.While))
            {
                return WhileStatement();
            }
            if (Match(TokenType.LeftBrace))
            {
                return new BlockStmt(Block());
            }

            return ExpressionStatement();
        }

        private ReturnStmt ReturnStatement()
        {
            var keyword = Previous();
            Expr value = null;
            if (!Check(TokenType.Semicolon))
            {
                value = Expression();
            }

            Consume(TokenType.Semicolon, "Expect ';' after return value.");
            return new ReturnStmt(keyword, value);
        }

        // Desugaring method that parses _for_ into _while_
        private Stmt ForStatement()
        {
            // Start by eating left parenthesis...
            Consume(TokenType.LeftParen, "Expect '(' after 'for'.");

            // ...consume initializer, if one is present...
            Stmt initializer;
            if (Match(TokenType.Semicolon))
            {
                initializer = null;
            }
            else if (Match(TokenType.Var))
            {
                initializer = VarDeclaration();
            }
            else
            {
                initializer = ExpressionStatement();
            }

            // ...consume loop condition, if one is present, and ensure that the semicolon is there...
            Expr condition = null;
            if (!Check(TokenType.Semicolon))
            {
                condition = Expression();
            }
            Consume(TokenType.Semicolon, "Expect ';' after loop condition.");

            // ...ditto for increment...
            Expr increment = null;
            if (!Check(TokenType.RightParen))
            {
                increment = Expression();
            }
            Consume(TokenType.RightParen, "Expect ')' after for clauses.");

            // ...and finally construct a body.
            var body = Statement();

            // Now the fun begins!

            // If increment was present, we put it a little block with the body,
            // in order to run increment once after every, well, loop
            if (increment != null)
            {
                body = new BlockStmt(new List<Stmt> { body, new ExpressionStmt(increment) });
            }

            // If condition was null, make a literal true, so we can get an infinite while
            if (condition is null)
            {
                condition = new LiteralExpr(true);
            }

            // Now we wrap the body into a while statment, with condition
            body = new WhileStmt(condition, body);

            // And finally another small block, this time with initializer before body
            // because initializer should only run once, of course
            if (initializer != null)
            {
                body = new BlockStmt(new List<Stmt> { initializer, body });
            }

            // And we can return the 'body' which is really full-fledged
            // while by now.
            return body;
        }

        private WhileStmt WhileStatement()
        {
            Consume(TokenType.LeftParen, "Expect '(' after 'while'.");
            var condition = Expression();
            Consume(TokenType.RightParen, "Expect ')' after condition.");
            var body = Statement();

            return new WhileStmt(condition, body);
        }

        private ConditionalStmt ConditionalStatement()
        {
            Consume(TokenType.LeftParen, "Expect '(' after 'if'.");
            var condition = Expression();
            Consume(TokenType.RightParen, "Expect ')' after if condition.");

            var thenBranch = Statement();
            Stmt elseBranch = null;
            if (Match(TokenType.Else))
            {
                elseBranch = Statement();
            }

            return new ConditionalStmt(condition, thenBranch, elseBranch);
        }

        private List<Stmt> Block()
        {
            var statements = new List<Stmt>();

            while (!Check(TokenType.RightBrace) && !IsAtEnd())
            {
                statements.Add(Declaration());
            }

            Consume(TokenType.RightBrace, "Expect '}' after block.");
            return statements;
        }

        private Stmt PrintStatement()
        {
            var value = Expression();
            Consume(TokenType.Semicolon, "Expect ';' after value.");
            return new PrintStmt(value);
        }

        private Stmt ExpressionStatement()
        {
            var expr = Expression();
            Consume(TokenType.Semicolon, "Expect ';' after value.");
            return new ExpressionStmt(expr);
        }

        private Expr Expression()
        {
            return Assignment();
        }

        private Expr Assignment()
        {
            var expr = Or();

            if (Match(TokenType.Equal))
            {
                // store token to allow error reporting
                var equals = Previous();
                // right-associative, so we call rule recursively
                var value = Assignment();

                // ensure that whatever we have parsed first is l-value...
                if (expr is VarExpr v)
                {
                    // ...and actually convert it to l-value...
                    var name = v.Name;
                    return new AssignmentExpr(name, value);
                }

                // ...or report error.
                Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Expr Or()
        {
            var expr = And();

            while (Match(TokenType.Or))
            {
                var op = Previous();
                var right = And();
                expr = new LogicalExpr(expr, op, right);
            }

            return expr;
        }

        private Expr And()
        {
            var expr = Equality();

            while (Match(TokenType.And))
            {
                var op = Previous();
                var right = Equality();
                expr = new LogicalExpr(expr, op, right);
            }

            return expr;
        }

        private Expr Equality()
        {
            Expr expr = Comparison();

            while (Match(TokenType.BangEqual, TokenType.DoubleEqual))
            {
                var op = Previous();
                var right = Comparison();
                expr = new BinaryExpr(expr, op, right);
            }

            return expr;
        }

        private Expr Comparison()
        {
            var expr = Addition();

            while (Match(TokenType.Greater, TokenType.GreaterOrEqual, TokenType.Less, TokenType.LessOrEqual))
            {
                var op = Previous();
                var right = Addition();
                expr = new BinaryExpr(expr, op, right);
            }

            return expr;
        }

        private Expr Addition()
        {
            var expr = Multiplication();

            while (Match(TokenType.Minus, TokenType.Plus))
            {
                var op = Previous();
                var right = Multiplication();
                expr = new BinaryExpr(expr, op, right);
            }

            return expr;
        }

        private Expr Multiplication()
        {
            var expr = Unary();

            while (Match(TokenType.Slash, TokenType.Star))
            {
                var op = Previous();
                var right = Unary();
                expr = new BinaryExpr(expr, op, right);
            }

            return expr;
        }

        private Expr Unary()
        {
            if (Match(TokenType.Bang, TokenType.Minus))
            {
                var op = Previous();
                var right = Unary();
                return new UnaryExpr(op, right);
            }

            return Call();
        }

        private Expr Call()
        {
            var expr = Primary();

            while (Match(TokenType.LeftParen))
            {
                expr = FinishCall(expr);
            }

            return expr;
        }

        private Expr FinishCall(Expr callee)
        {
            var arguments = new List<Expr>();
            if (!Check(TokenType.RightParen))
            {
                do
                {
                    if (arguments.Count >= 255)
                    {
                        Error(Peek(), "Cannot pass more than 255 arguments to function invocation.");
                    }
                    arguments.Add(Expression());
                } while (Match(TokenType.Comma));
            }

            var paren = Consume(TokenType.RightParen, "Expect ')' after arguments.");

            return new CallExpr(callee, paren, arguments);
        }

        private Expr Primary()
        {
            if (Match(TokenType.False))
            {
                return new LiteralExpr(false);
            }
            if (Match(TokenType.True))
            {
                return new LiteralExpr(true);
            }
            if (Match(TokenType.Nil))
            {
                return new LiteralExpr(null);
            }

            if (Match(TokenType.Number, TokenType.String))
            {
                return new LiteralExpr(Previous().Literal);
            }

            if (Match(TokenType.Identifier))
            {
                return new VarExpr(Previous());
            }

            if (Match(TokenType.LeftParen))
            {
                var expr = Expression();
                Consume(TokenType.RightParen, "Expected ')' after expression.");
                return new GroupingExpr(expr);
            }

            throw Error(Peek(), "Expect expression");
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type))
            {
                return Advance();
            }

            throw Error(Peek(), message);
        }

        private ParseException Error(Token token, string message)
        {
            Program.Error(token, message);
            return new ParseException(message);
        }

        private bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private bool Check(TokenType type) => !IsAtEnd() && Peek().Type == type;

        private Token Advance()
        {
            current += IsAtEnd() ? 0 : 1;

            return Previous();
        }

        private bool IsAtEnd() => Peek().Type == TokenType.EOF;

        private Token Previous() => tokens[current - 1];

        private Token Peek() => tokens[current];

        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if (Previous().Type == TokenType.Semicolon)
                {
                    return;
                }

                switch (Peek().Type)
                {
                    case TokenType.Class:
                    case TokenType.Fun:
                    case TokenType.Var:
                    case TokenType.For:
                    case TokenType.If:
                    case TokenType.While:
                    case TokenType.Print:
                    case TokenType.Return:
                        return;
                }
            }

            Advance();
        }
    }
}
