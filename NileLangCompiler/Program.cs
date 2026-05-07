using System;
using System.IO;
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


        // =========================================================
        // FULL TRANSPILATION DEMO
        // =========================================================
        Console.WriteLine("\n\n##################################################");
        Console.WriteLine("       FULL TRANSPILATION: NileLang → C#");
        Console.WriteLine("##################################################\n");

        string fullProgram = @"
            dynasty calculate(stone a, stone b) {
                tribute a * b;
            }

            stone result = calculate(10, 5);
            carve(result);

            judge (result > 30) {
                carve(""Big number!"");
            } banish {
                carve(""Small number."");
            }

            stone counter = 0;
            flow (counter < 5) {
                carve(counter);
                counter = counter + 1;
            }
        ";

        RunFullTranspile(fullProgram);
    }


    static void RunFullTranspile(string sourceCode)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("[ NileLang Source ]\n");
        Console.ResetColor();
        Console.WriteLine(sourceCode.Trim());
        Console.WriteLine("\n--------------------------------------------------\n");

        try
        {
            // 1. Lexical Analysis (Scanner)
            Scanner scanner = new Scanner();
            var tokens = scanner.Scan(sourceCode);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Phase 1  SCANNING ............... PASSED");
            Console.ResetColor();

            // 2. Syntax Analysis (Parser)
            Parser parser = new Parser(tokens);
            List<Stmt> ast = parser.Parse();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Phase 2  PARSING ................ PASSED");
            Console.ResetColor();

            // 3. Semantic Analysis
            SemanticAnalyzer analyzer = new SemanticAnalyzer();
            analyzer.Analyze(ast);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Phase 3  SEMANTIC ANALYSIS ...... PASSED");
            Console.ResetColor();

            // 4. Code Generation (Transpile to C#)
            CSharpEmitter emitter = new CSharpEmitter();
            string csharpCode = emitter.Emit(ast);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Phase 4  CODE GENERATION ........ PASSED");
            Console.ResetColor();

            // Output to console
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n[ Generated C# Code ]\n");
            Console.ResetColor();
            Console.WriteLine(csharpCode);

            // Output to file
            string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "output.cs");
            File.WriteAllText(outputPath, csharpCode);
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"C# code written to: {outputPath}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[ TRANSPILATION FAILED ]\n{ex.Message}");
            Console.ResetColor();
        }
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