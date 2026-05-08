using System;
using System.IO;
using System.Collections.Generic;
using NileLangCompiler;

class Program
{
    static void Main()
    {
        // ═══════════════════════════════════════════════════════════
        //  THE NILELANG PROGRAM
        // ═══════════════════════════════════════════════════════════

        string sourceCode = @"
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

        Console.WriteLine("╔══════════════════════════════════════════════════╗");
        Console.WriteLine("║         𓂀  NileLang Compiler Pipeline  𓂀         ║");
        Console.WriteLine("╚══════════════════════════════════════════════════╝");
        Console.WriteLine();

        // ─────────────────────────────────────────────────────────
        //  Show the source code
        // ─────────────────────────────────────────────────────────
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("┌──────────────────────────────────────────────────┐");
        Console.WriteLine("│              NileLang Source Code                │");
        Console.WriteLine("└──────────────────────────────────────────────────┘");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine(sourceCode.Trim());
        Console.WriteLine();


        // ═════════════════════════════════════════════════════════
        //  PHASE 1 — LEXICAL ANALYSIS (Scanner)
        // ═════════════════════════════════════════════════════════
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("══════════════════════════════════════════════════");
        Console.WriteLine("  Phase 1 ─ LEXICAL ANALYSIS (Scanner)");
        Console.WriteLine("══════════════════════════════════════════════════");
        Console.ResetColor();
        Console.WriteLine();

        Scanner scanner = new Scanner();
        List<Token> tokens = scanner.Scan(sourceCode);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("  ✓ Scanning complete — tokens produced:\n");
        Console.ResetColor();

        // Print the token stream in a formatted table
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  {0,-6} {1,-20} {2}", "#", "Lexeme", "Token Type");
        Console.WriteLine("  " + new string('─', 50));
        Console.ResetColor();

        for (int i = 0; i < tokens.Count; i++)
        {
            string lexeme = tokens[i].Lexeme;
            if (string.IsNullOrWhiteSpace(lexeme)) lexeme = "(EOF)";
            // Truncate long strings for display
            if (lexeme.Length > 18) lexeme = lexeme.Substring(0, 15) + "...";

            Console.WriteLine($"  {i + 1,-6} {lexeme,-20} {tokens[i].Type}");
        }
        Console.WriteLine();


        // ═════════════════════════════════════════════════════════
        //  PHASE 2 — SYNTAX ANALYSIS (Parser)
        // ═════════════════════════════════════════════════════════
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("══════════════════════════════════════════════════");
        Console.WriteLine("  Phase 2 ─ SYNTAX ANALYSIS (Parser)");
        Console.WriteLine("══════════════════════════════════════════════════");
        Console.ResetColor();
        Console.WriteLine();

        Parser parser = new Parser(tokens);
        List<Stmt> ast = parser.Parse();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("  ✓ Parsing complete — Abstract Syntax Tree:\n");
        Console.ResetColor();

        AstPrinter.PrintTree(ast);
        Console.WriteLine();


        // ═════════════════════════════════════════════════════════
        //  PHASE 3 — SEMANTIC ANALYSIS
        // ═════════════════════════════════════════════════════════
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("══════════════════════════════════════════════════");
        Console.WriteLine("  Phase 3 ─ SEMANTIC ANALYSIS");
        Console.WriteLine("══════════════════════════════════════════════════");
        Console.ResetColor();
        Console.WriteLine();

        SemanticAnalyzer analyzer = new SemanticAnalyzer();
        analyzer.Analyze(ast);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("  ✓ All type checks passed");
        Console.WriteLine("  ✓ All scopes validated");
        Console.WriteLine("  ✓ All function signatures verified");
        Console.WriteLine("  ✓ Control flow is correct");
        Console.ResetColor();
        Console.WriteLine();


        // ═════════════════════════════════════════════════════════
        //  PHASE 4 — CODE GENERATION (Transpile to C#)
        // ═════════════════════════════════════════════════════════
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("══════════════════════════════════════════════════");
        Console.WriteLine("  Phase 4 ─ CODE GENERATION (NileLang → C#)");
        Console.WriteLine("══════════════════════════════════════════════════");
        Console.ResetColor();
        Console.WriteLine();

        CSharpEmitter emitter = new CSharpEmitter();
        string csharpCode = emitter.Emit(ast);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("  ✓ C# code generated:\n");
        Console.ResetColor();

        // Print the generated C# with line numbers
        string[] lines = csharpCode.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"  {i + 1,3} │ ");
            Console.ResetColor();
            Console.WriteLine(lines[i].TrimEnd('\r'));
        }

        // Save to file
        string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "output.cs");
        File.WriteAllText(outputPath, csharpCode);
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"  📄 Saved to: {outputPath}");
        Console.ResetColor();
        Console.WriteLine();


        // ═════════════════════════════════════════════════════════
        //  PHASE 5 — EXECUTION (Tree-Walking Interpreter)
        // ═════════════════════════════════════════════════════════
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("══════════════════════════════════════════════════");
        Console.WriteLine("  Phase 5 ─ EXECUTION (Interpreter)");
        Console.WriteLine("══════════════════════════════════════════════════");
        Console.ResetColor();
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("  ✓ Interpreter running...\n");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("  ┌──────── Program Output ────────┐");
        Console.ResetColor();
        Console.WriteLine();

        NileInterpreter interpreter = new NileInterpreter();
        interpreter.Execute(ast);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("  └──────── End of Output ─────────┘");
        Console.ResetColor();
        Console.WriteLine();


        // ═════════════════════════════════════════════════════════
        //  SUMMARY
        // ═════════════════════════════════════════════════════════
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("╔══════════════════════════════════════════════════╗");
        Console.WriteLine("║           ✓ All 5 Phases Complete               ║");
        Console.WriteLine("╠══════════════════════════════════════════════════╣");
        Console.WriteLine("║  Phase 1 │ Lexical Analysis    ........  PASSED ║");
        Console.WriteLine("║  Phase 2 │ Syntax Analysis     ........  PASSED ║");
        Console.WriteLine("║  Phase 3 │ Semantic Analysis   ........  PASSED ║");
        Console.WriteLine("║  Phase 4 │ Code Generation     ........  PASSED ║");
        Console.WriteLine("║  Phase 5 │ Execution           ........  PASSED ║");
        Console.WriteLine("╚══════════════════════════════════════════════════╝");
        Console.ResetColor();
    }
}