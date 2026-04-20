using System;
using System.Collections.Generic;

namespace NileLangCompiler;

public class Parser
{
    private readonly List<Token> _tokens;
    private int _current = 0;

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    // ==========================================
    // 1. THE HELPER TOOLS (The Parser's Eyes)
    // ==========================================
    
    private Token Peek() => _tokens[_current];
    private Token Previous() => _tokens[_current - 1];
    private bool IsAtEnd() => Peek().Type == TokenType.EOF;

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) _current++;
        return Previous();
    }

    private Token Expect(TokenType type, string message)
    {
        if (Check(type)) return Advance();
        throw new Exception($"Syntax Error [Line {Peek().Line}]: {message}. Found '{Peek().Lexeme}' instead.");
    }

   
    // ==========================================
    // 2. STATEMENTS & CONTROL FLOW
    // ==========================================

    public void Parse()
    {
        Console.WriteLine("\n--- Starting Syntax Analysis (Parsing) ---");
        try
        {
            while (!IsAtEnd())
            {
                ParseStatement();
            }
            Console.WriteLine("\nSUCCESS: Parsing Completed! No Syntax Errors.");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n" + ex.Message);
            Console.ResetColor();
        }
    }

    // The Ultimate Traffic Cop
    private void ParseStatement()
    {
        if (Check(TokenType.Stone) || Check(TokenType.Water) || Check(TokenType.Papyrus) || Check(TokenType.Maat))
        {
            ParseVariableDeclaration();
        }
        else if (Check(TokenType.Judge))
        {
            ParseIfStatement();
        }
        else if (Check(TokenType.Flow))
        {
            ParseWhileStatement();
        }
        else if (Check(TokenType.Carve))
        {
            ParsePrintStatement();
        }
        else if (Check(TokenType.LeftBrace))
        {
            ParseBlock();
        }
        else if (Check(TokenType.Identifier))
        {
            ParseAssignmentStatement();
        }
        else
        {
            throw new Exception($"Syntax Error: Unrecognized statement. Found '{Peek().Lexeme}'");
        }
    }

    private void ParseVariableDeclaration()
    {
        Token typeToken = Advance(); 
        Token nameToken = Expect(TokenType.Identifier, "Expected a variable name after the type.");
        Expect(TokenType.Assign, $"Expected '=' after variable name '{nameToken.Lexeme}'.");
        
        ParseExpression(); // Evaluates math, strings, or logic
        
        Expect(TokenType.Semicolon, "Expected ';' at the end of the variable declaration.");
        Console.WriteLine($"[Valid]: Variable '{nameToken.Lexeme}' declared.");
    }

    private void ParseAssignmentStatement()
    {
        Token nameToken = Advance(); 
        Expect(TokenType.Assign, $"Expected '=' after variable name '{nameToken.Lexeme}'.");
        
        ParseExpression();
        
        Expect(TokenType.Semicolon, "Expected ';' at the end of the assignment.");
        Console.WriteLine($"[Valid]: Assigned new value to '{nameToken.Lexeme}'.");
    }

    private void ParseBlock()
    {
        Expect(TokenType.LeftBrace, "Expected '{' to start a block.");
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            ParseStatement(); 
        }
        Expect(TokenType.RightBrace, "Expected '}' to end the block.");
        Console.WriteLine("[Valid]: Block parsed successfully.");
    }

    // IF / ELSE (judge / banish)
    private void ParseIfStatement()
    {
        Advance(); // Consume 'judge'
        Expect(TokenType.LeftParen, "Expected '(' after 'judge'.");
        
        ParseExpression(); // The condition (e.g., x > 5)
        
        Expect(TokenType.RightParen, "Expected ')' after judge condition.");
        
        ParseBlock(); // The 'if' body
        
        // Optional 'Else' (banish)
        if (Check(TokenType.Banish))
        {
            Advance(); // Consume 'banish'
            ParseBlock(); // The 'else' body
        }
        Console.WriteLine("[Valid]: 'Judge' (If) statement parsed.");
    }

    // WHILE LOOP (flow)
    private void ParseWhileStatement()
    {
        Advance(); // Consume 'flow'
        Expect(TokenType.LeftParen, "Expected '(' after 'flow'.");
        
        ParseExpression(); // The condition
        
        Expect(TokenType.RightParen, "Expected ')' after flow condition.");
        
        ParseBlock(); // The loop body
        
        Console.WriteLine("[Valid]: 'Flow' (While) loop parsed.");
    }

    // PRINT (carve)
    private void ParsePrintStatement()
    {
        Advance(); // Consume 'carve'
        Expect(TokenType.LeftParen, "Expected '(' after 'carve'.");
        
        ParseExpression(); // What to print
        
        Expect(TokenType.RightParen, "Expected ')' after print value.");
        Expect(TokenType.Semicolon, "Expected ';' at the end of the carve statement.");
        
        Console.WriteLine("[Valid]: 'Carve' (Print) statement parsed.");
    }

    // ==========================================
    // 3. FULL EXPRESSIONS ENGINE (PEMDAS + Logic)
    // ==========================================

    private void ParseExpression()
    {
        ParseLogicOr();
    }

    private void ParseLogicOr()
    {
        ParseLogicAnd();
        while (Check(TokenType.Or)) // ||
        {
            Advance();
            ParseLogicAnd();
        }
    }

    private void ParseLogicAnd()
    {
        ParseEquality();
        while (Check(TokenType.And)) // &&
        {
            Advance();
            ParseEquality();
        }
    }

    private void ParseEquality()
    {
        ParseComparison();
        while (Check(TokenType.Equals) || Check(TokenType.NotEquals)) // ==, !=
        {
            Advance();
            ParseComparison();
        }
    }

    private void ParseComparison()
    {
        ParseTerm();
        while (Check(TokenType.GreaterThan) || Check(TokenType.GreaterOrEqual) || 
               Check(TokenType.LessThan) || Check(TokenType.LessOrEqual)) // >, >=, <, <=
        {
            Advance();
            ParseTerm();
        }
    }

    private void ParseTerm()
    {
        ParseFactor(); 
        while (Check(TokenType.Plus) || Check(TokenType.Minus)) // +, -
        {
            Advance(); 
            ParseFactor(); 
        }
    }

    private void ParseFactor()
    {
        ParseUnary(); 
        while (Check(TokenType.Multiply) || Check(TokenType.Divide)) // *, /
        {
            Advance(); 
            ParseUnary(); 
        }
    }

    private void ParseUnary()
    {
        if (Check(TokenType.Not) || Check(TokenType.Minus)) // !, - (e.g., !true or -5)
        {
            Advance();
            ParseUnary();
            return;
        }
        ParsePrimary();
    }

    private void ParsePrimary()
    {
        // Numbers, Strings, and Variable Names
        if (Check(TokenType.Integer) || Check(TokenType.Float) || 
            Check(TokenType.StringLiteral) || Check(TokenType.Identifier))
        {
            Advance(); 
            return;
        }

        // Grouping logic with Parentheses
        if (Check(TokenType.LeftParen))
        {
            Advance(); 
            ParseExpression(); 
            Expect(TokenType.RightParen, "Expected ')' after expression.");
            return;
        }

        throw new Exception($"Syntax Error [Line {Peek().Line}]: Expected a value, variable, or '('. Found '{Peek().Lexeme}'.");
    }
}