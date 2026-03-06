using System;
using NileLangCompiler;

class Program
{
    static void Main()
    {
        // A snippet of NileLang code based on your Underworld example
        string sourceCode = @"
            // The Pharaoh commanded this variable
            stone goodDeeds = 10;
            @
        ";

        Console.WriteLine("Scanning NileLang Source Code...\n");

        Scanner scanner = new Scanner();
        var tokens = scanner.Scan(sourceCode);

        // Print out every token the scanner found
        foreach (var token in tokens)
        {
            Console.WriteLine(token.ToString());
        }
    }
}
