using System;
using System.IO;
using NLox.AST;
using NLox.Scanner;

namespace NLox
{
    internal static class Program
    {
        private static bool hadError = false;
        private static bool hadRuntimeError = false;

        private static int Main(string[] args)
        {
            return args.Length switch
            {
                var n when n > 1 => DisplayHelp(),
                1 => RunFile(args[0]),
                _ => RunPrompt()
            };
        }

        private static int DisplayHelp()
        {
            Console.WriteLine("Usage: nlox [script]");
            return 64;
        }

        private static int RunFile(string path)
        {
            var script = File.ReadAllText(path);
            return Run(script);
        }

        private static int RunPrompt()
        {
            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                Run(line);
                hadError = false;
            }
        }

        private static int Run(string source)
        {
            var scanner = new Scanner.Scanner(source);
            var tokens = scanner.ScanTokens();
            var parser = new Parser(tokens);
            var stmts = parser.Parse();

            if (hadError) return 65;

            Interpreter.Interpret(stmts);

            if (hadRuntimeError) return 70;

            return 0;
        }

        public static void Error(int line, string message)
        {
            Report(line, string.Empty, message);
        }

        public static void Error(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
            {
                Report(token.Line, " at end", message);
            }
            else
            {
                Report(token.Line, $" at '{token.Lexeme}'", message);
            }
        }

        public static void RuntimeError(RuntimeException rex)
        {
            Console.WriteLine($"{rex.Message}\n[line {rex.token.Line}]");
            hadRuntimeError = true;
        }

        private static void Report(int line, string where, string message)
        {
            Console.WriteLine($"[line {line}] Error {where}: {message}");
            hadError = true;
        }
    }
}
