using System;
using System.Collections.Generic;
using NileLangCompiler;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== NileLang Compiler Diagnostics ===\n");

        // ---------------------------------------------------------
        // EXAMPLE 1: PERFECT CODE
        // ---------------------------------------------------------
        string code1 = @"
            dynasty calculate(stone a, stone b) { tribute a * b; }
            stone result = calculate(10, 5);
        ";
        RunDiagnostic("EXAMPLE 1: Valid Code", code1);


        // ---------------------------------------------------------
        // EXAMPLE 2: SYNTAX ERROR (Parser breaks)
        // ---------------------------------------------------------
        string code2 = @"
            stone x = 10 /* Missing semicolon! */
            stone y = 20;
        ";
        RunDiagnostic("EXAMPLE 2: Missing Semicolon", code2);


        // ---------------------------------------------------------
        // EXAMPLE 3: SEMANTIC ERROR (Type Mismatch)
        // ---------------------------------------------------------
        string code3 = @"
            stone number = 10;
            papyrus word = ""Hello"";
            stone badMath = number + word;
        ";
        RunDiagnostic("EXAMPLE 3: Type Mismatch", code3);


        // ---------------------------------------------------------
        // EXAMPLE 4: SEMANTIC ERROR (Function Signature)
        // ---------------------------------------------------------
        string code4 = @"
            dynasty build(stone blocks) { }
            papyrus badArg = ""text"";
            stone dummy = build(badArg);
        ";
        RunDiagnostic("EXAMPLE 4: Signature Mismatch", code4);
    }

    static void RunDiagnostic(string testName, string sourceCode)
    {
        Console.WriteLine($"\n==================================================");
        Console.WriteLine($" {testName.ToUpper()}");
        Console.WriteLine($"==================================================");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(sourceCode.Trim());
        Console.ResetColor();
        Console.WriteLine("--------------------------------------------------");

        try 
        {
            // 1. Lexical Analysis
            Scanner scanner = new Scanner();
            var tokens = scanner.Scan(sourceCode);

            // 2. Syntax Analysis (Parser)
            Parser parser = new Parser(tokens);
            List<Stmt> ast = parser.Parse();

            // --- PRINT PARSER OUTPUT ---
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n[ PARSER OUTPUT: Abstract Syntax Tree ]");
            Console.ResetColor();
            AstPrinter.PrintTree(ast);

            // 3. Static Analysis (Semantic Phase)
            // --- PRINT SEMANTIC OUTPUT ---
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n[ SEMANTIC ANALYZER OUTPUT ]");
            Console.ResetColor();
            
            SemanticAnalyzer analyzer = new SemanticAnalyzer();
            analyzer.Analyze(ast);
            
            // If it reaches this line, no exceptions were thrown!
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("SUCCESS: Semantic Analysis passed! The code is structurally and logically safe.");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            // If the Parser OR Semantic Analyzer throws an error, it prints here.
            Console.ForegroundColor = ConsoleColor.Red;
            
            if (ex.Message.Contains("Syntax Error") || ex.Message.Contains("Expected"))
            {
                Console.WriteLine("\n[ PARSER CRASHED ]");
            }
            else if (ex.Message.Contains("Semantic Error"))
            {
                Console.WriteLine("\n[ SEMANTIC ANALYZER CRASHED ]");
            }
            else
            {
                Console.WriteLine("\n[ FATAL COMPILER ERROR ]");
            }
            
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }
        Console.WriteLine("\n");
    }
}