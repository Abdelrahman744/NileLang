using System;
using NileLangCompiler;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== The Great Scribe's Testing Chamber ===\n");

        RunTest("TEST 1: Variables & Complex Math", TestMath());
        RunTest("TEST 2: Judge & Banish (If / Else)", TestIfElse());
        RunTest("TEST 3: Flow (While Loop)", TestLoop());
        RunTest("TEST 4: Carve (Print Statement)", TestPrint());
        RunTest("TEST 5: The Guardrail (Intentional Syntax Error)", TestError());
    }

    static void RunTest(string testName, string sourceCode)
    {
        Console.WriteLine($"\n========== {testName} ==========");
        Console.WriteLine("Source Code:");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(sourceCode.Trim());
        Console.ResetColor();
        Console.WriteLine("--------------------------------");

        // 1. Lexical Analysis (The Scanner)
        Scanner scanner = new Scanner();
        var tokens = scanner.Scan(sourceCode);

        // 2. Syntax Analysis (The Parser)
        Parser parser = new Parser(tokens);
        parser.Parse();
        
        Console.WriteLine("==========================================\n");
    }

    // ---------------------------------------------------------
    // TEST CASES
    // ---------------------------------------------------------

    static string TestMath()
    {
        return @"
            stone workers = 10 + 5 * 2;
            water depth = (15.5 - 5.0) / 2.0;
            workers = workers + 1;
        ";
    }

    static string TestIfElse()
    {
        return @"
            judge (workers >= 20 && depth != 0.0) {
                stone bonus = 5;
                workers = workers + bonus;
            } banish {
                workers = 0;
            }
        ";
    }

    static string TestLoop()
    {
        return @"
            stone blocks = 0;
            flow (blocks < 1000) {
                blocks = blocks + 10;
            }
        ";
    }

    static string TestPrint()
    {
        return @"
            papyrus message = ""Building the pyramid"";
            carve(message);
        ";
    }

   static string TestError()
    {
        return @"
            stone missingSemicolon = 10
            
            papyrus safeStatement = ""The parser recovered!"";
            
            judge (brokenCondition {
                stone x = 5;
            }
        ";
    }
}