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
    // 1. HELPER TOOLS
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

    private void Synchronize()
    {
        Advance();
        while (!IsAtEnd())
        {
            if (Previous().Type == TokenType.Semicolon) return;
            switch (Peek().Type)
            {
                case TokenType.Stone:
                case TokenType.Water:
                case TokenType.Papyrus:
                case TokenType.Maat:
                case TokenType.Judge:
                case TokenType.Flow:
                case TokenType.Carve:
                    return;
            }
            Advance();
        }
    }

    // ==========================================
    // 2. STATEMENTS (Return Stmt objects)
    // ==========================================

    public List<Stmt> Parse()
    {
        List<Stmt> statements = new List<Stmt>();
        bool hadError = false;

        Console.WriteLine("\n--- Starting Syntax Analysis & AST Building ---");
        
        while (!IsAtEnd())
        {
            try
            {
                statements.Add(ParseStatement());
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n" + ex.Message);
                Console.ResetColor();
                hadError = true;
                Synchronize();
            }
        }

        if (!hadError) Console.WriteLine("SUCCESS: AST Built Perfectly!");
        else Console.WriteLine("WARNING: AST built partially due to syntax errors.");

        return statements;
    }

    private Stmt ParseStatement()
    {
        if (Check(TokenType.Stone) || Check(TokenType.Water) || Check(TokenType.Papyrus) || Check(TokenType.Maat))
            return ParseVariableDeclaration();
        if (Check(TokenType.Judge)) return ParseIfStatement();
        if (Check(TokenType.Flow)) return ParseWhileStatement();
        if (Check(TokenType.Carve)) return ParsePrintStatement();
        if (Check(TokenType.LeftBrace)) return ParseBlock();
        if (Check(TokenType.Identifier)) return ParseAssignmentStatement();

        throw new Exception($"Syntax Error: Unrecognized statement. Found '{Peek().Lexeme}'");
    }

    private Stmt ParseVariableDeclaration()
    {
        Token typeToken = Advance();
        Token nameToken = Expect(TokenType.Identifier, "Expected a variable name.");
        Expect(TokenType.Assign, "Expected '=' after variable name.");
        
        Expr initializer = ParseExpression();
        
        Expect(TokenType.Semicolon, "Expected ';' at the end of declaration.");
        return new VarDeclStmt(typeToken, nameToken, initializer);
    }

    private Stmt ParseAssignmentStatement()
    {
        Token nameToken = Advance();
        Expect(TokenType.Assign, "Expected '=' after variable name.");
        Expr value = ParseExpression();
        Expect(TokenType.Semicolon, "Expected ';' at the end of assignment.");
        return new AssignStmt(nameToken, value);
    }

    private Stmt ParseBlock()
    {
        Expect(TokenType.LeftBrace, "Expected '{' to start a block.");
        List<Stmt> statements = new List<Stmt>();

        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            statements.Add(ParseStatement());
        }

        Expect(TokenType.RightBrace, "Expected '}' to end the block.");
        return new BlockStmt(statements);
    }

    private Stmt ParseIfStatement()
    {
        Advance(); // 'judge'
        Expect(TokenType.LeftParen, "Expected '(' after 'judge'.");
        Expr condition = ParseExpression();
        Expect(TokenType.RightParen, "Expected ')' after condition.");
        
        Stmt thenBranch = ParseBlock();
        Stmt elseBranch = null;

        if (Check(TokenType.Banish))
        {
            Advance(); // 'banish'
            elseBranch = ParseBlock();
        }

        return new IfStmt(condition, thenBranch, elseBranch);
    }

    private Stmt ParseWhileStatement()
    {
        Advance(); // 'flow'
        Expect(TokenType.LeftParen, "Expected '(' after 'flow'.");
        Expr condition = ParseExpression();
        Expect(TokenType.RightParen, "Expected ')' after condition.");
        Stmt body = ParseBlock();
        return new WhileStmt(condition, body);
    }

    private Stmt ParsePrintStatement()
    {
        Advance(); // 'carve'
        Expect(TokenType.LeftParen, "Expected '(' after 'carve'.");
        Expr value = ParseExpression();
        Expect(TokenType.RightParen, "Expected ')' after value.");
        Expect(TokenType.Semicolon, "Expected ';'.");
        return new PrintStmt(value);
    }

    // ==========================================
    // 3. EXPRESSIONS (Return Expr objects)
    // ==========================================

    private Expr ParseExpression() => ParseLogicOr();

    private Expr ParseLogicOr()
    {
        Expr expr = ParseLogicAnd();
        while (Check(TokenType.Or))
        {
            Token op = Advance();
            Expr right = ParseLogicAnd();
            expr = new LogicalExpr(expr, op, right);
        }
        return expr;
    }

    private Expr ParseLogicAnd()
    {
        Expr expr = ParseEquality();
        while (Check(TokenType.And))
        {
            Token op = Advance();
            Expr right = ParseEquality();
            expr = new LogicalExpr(expr, op, right);
        }
        return expr;
    }

    private Expr ParseEquality()
    {
        Expr expr = ParseComparison();
        while (Check(TokenType.Equals) || Check(TokenType.NotEquals))
        {
            Token op = Advance();
            Expr right = ParseComparison();
            expr = new BinaryExpr(expr, op, right);
        }
        return expr;
    }

    private Expr ParseComparison()
    {
        Expr expr = ParseTerm();
        while (Check(TokenType.GreaterThan) || Check(TokenType.GreaterOrEqual) || 
               Check(TokenType.LessThan) || Check(TokenType.LessOrEqual))
        {
            Token op = Advance();
            Expr right = ParseTerm();
            expr = new BinaryExpr(expr, op, right);
        }
        return expr;
    }

    private Expr ParseTerm()
    {
        Expr expr = ParseFactor();
        while (Check(TokenType.Plus) || Check(TokenType.Minus))
        {
            Token op = Advance();
            Expr right = ParseFactor();
            expr = new BinaryExpr(expr, op, right); // <--- Building the Tree!
        }
        return expr;
    }

    private Expr ParseFactor()
    {
        Expr expr = ParseUnary();
        while (Check(TokenType.Multiply) || Check(TokenType.Divide))
        {
            Token op = Advance();
            Expr right = ParseUnary();
            expr = new BinaryExpr(expr, op, right);
        }
        return expr;
    }

    private Expr ParseUnary()
    {
        if (Check(TokenType.Not) || Check(TokenType.Minus))
        {
            Token op = Advance();
            Expr right = ParseUnary();
            return new UnaryExpr(op, right);
        }
        return ParsePrimary();
    }

    private Expr ParsePrimary()
    {
        if (Check(TokenType.Integer))
            return new LiteralExpr(int.Parse(Advance().Lexeme));
            
        if (Check(TokenType.Float))
            return new LiteralExpr(double.Parse(Advance().Lexeme));

        if (Check(TokenType.StringLiteral))
        {
            string text = Advance().Lexeme;
            // Remove the quotes around the string for the raw value
            return new LiteralExpr(text.Substring(1, text.Length - 2)); 
        }

        if (Check(TokenType.Identifier))
            return new VariableExpr(Advance());

        if (Check(TokenType.LeftParen))
        {
            Advance();
            Expr expr = ParseExpression();
            Expect(TokenType.RightParen, "Expected ')' after expression.");
            return expr; // Grouping is naturally handled by the tree structure!
        }

        throw new Exception($"Syntax Error: Expected a value. Found '{Peek().Lexeme}'.");
    }
}