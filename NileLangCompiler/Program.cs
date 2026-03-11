using System;
using NileLangCompiler;

class Program
{
    static void Main()
    {
       string a = @"water nileDepth = 15.5;
                    stone builders = 100;
                    
                    ";
       string sourceCode = a;

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
