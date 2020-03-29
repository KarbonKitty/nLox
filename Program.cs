using System;
using System.IO;

namespace NLox
{
    internal static class Program
    {
        private static bool hadError = false;

        private static int Main(string[] args)
        {
            return args.Length switch {
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
            var scanner = new Scanner(source);
            var tokens = scanner.ScanTokens();

            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }

            return 0;
        }

        public static void Error(int line, string message)
        {
            Report(line, string.Empty, message);
        }

        private static void Report(int line, string where, string message)
        {
            Console.WriteLine($"[line {line}] Error {where}: {message}");
            hadError = true;
        }
    }
}
