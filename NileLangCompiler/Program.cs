using System;
using System.Collections.Generic;
using NileLangCompiler;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== The Master Scribe's Final Evaluation ===\n");

        string masterScript = @"
            /* =========================================
               1. VARIABLES & DATA TYPES
               ========================================= */
            stone workers = 10;
            water rations = 15.5;
            maat isRaining = false;
            papyrus greeting = ""Welcome to the Nile construction site!"";

            carve(greeting);

            /* =========================================
               2. FUNCTIONS & RETURNS (Semantic Signatures)
               ========================================= */
            dynasty calculateBricks(stone layers, stone bricksPerLayer) {
                stone total = layers * bricksPerLayer;
                tribute total; /* Return the math */
            }

            /* =========================================
               3. CONTROL FLOW, MATH & FUNCTION CALLS
               ========================================= */
            judge (workers >= 10 && !isRaining) {
                carve(""Conditions are perfect. Calculating bricks..."");
                
                /* Calling the function! */
                stone totalBricks = calculateBricks(5, 20);
                
                carve(""Total bricks needed:"");
                carve(totalBricks);
            } banish {
                carve(""Construction halted due to weather or lack of workers."");
            }

            /* =========================================
               4. LOOPS, PERSIST & SHATTER
               ========================================= */
            stone block = 0;
            carve(""--- Starting block transportation ---"");
            
            flow (block < 5) {
                block = block + 1;
                
                judge (block == 3) {
                    carve(""Block 3 is damaged. Persist! (Skipping...)"");
                    persist;
                }
                
                carve(""Moved block number:"");
                carve(block);

                judge (block == 4) {
                    carve(""Quota reached for the day. Shatter! (Breaking...)"");
                    shatter;
                }
            }
            carve(""--- Transportation Complete ---"");

            /* =========================================
               5. SEMANTIC GUARDRAIL TESTS 
               (Remove the comment slashes '/*' to test!)
               ========================================= */
            
            /* Guardrail 1: Type Mismatch */
             stone badMath = workers + greeting;

            /* Guardrail 2: Function Signature Mismatch (Passing Papyrus to Stone) */
             stone badCall = calculateBricks(5, greeting); 

            /* Guardrail 3: Undeclared Variable */
            carve(phantomVariable); 
        ";

        RunTest("NILELANG MASTER SUITE", masterScript);
    }

    static void RunTest(string testName, string sourceCode)
    {
        Console.WriteLine($"\n========== {testName} ==========");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(sourceCode.Trim());
        Console.ResetColor();
        Console.WriteLine("--------------------------------");

        try 
        {
            // 1. Scanner (Lexical Analysis)
            Scanner scanner = new Scanner();
            var tokens = scanner.Scan(sourceCode);

            // 2. Parser (Syntax Analysis - Fail Fast!)
            Parser parser = new Parser(tokens);
            List<Stmt> abstractSyntaxTree = parser.Parse();

            // 3. Print AST (Visualizing the Parser's Brain)
            Console.WriteLine($"[AST Size]: Created {abstractSyntaxTree.Count} root nodes.");
            AstPrinter.PrintTree(abstractSyntaxTree);

            // 4. Evaluator (Semantic Analysis & Execution)
            Evaluator evaluator = new Evaluator();
            evaluator.Interpret(abstractSyntaxTree);
        }
        catch (Exception ex)
        {
            // If the Parser OR Semantic Analyzer catches a fatal error, it lands here!
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[FATAL COMPILATION ERROR]:");
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }
        
        Console.WriteLine("==========================================\n");
    }
}