using System;
using System.IO;
using NileLangCompiler;

class Program
{
    static void Main(string[] args)
    {
        string sourceCode;

        if (args.Length > 0)
        {
            string filePath = args[0];
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"[NileLang Error] File not found: {filePath}");
                return;
            }
            sourceCode = File.ReadAllText(filePath);
        }
        else
        {
            // Default demo code if no file is given
            Console.WriteLine("[NileLang] No file provided. Running default demo...\n");
            sourceCode = @"
                dynasty greet() {
                    carve 42;
                }
                greet();
            ";
        }

        try
        {
            // YOUR EXISTING PIPELINE GOES HERE — don't change this part
            var scanner = new Scanner(sourceCode);
            var tokens = scanner.Tokenize();

            var parser = new Parser(tokens);
            var statements = parser.Parse();

            var interpreter = new NileInterpreter();
            interpreter.Interpret(statements);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NileLang Error] {ex.Message}");
        }
    }
}