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



    // 1. HELPER TOOLS
  
    private Token Peek() => _tokens[_current]; // lookahead token
    private Token Previous() => _tokens[_current - 1]; // recently consumed token
    private bool IsAtEnd() => Peek().Type == TokenType.EOF;

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }

    private Token Advance() // move forward and return the consumed token
    {
        if (!IsAtEnd()) _current++;
        return Previous();
    }

    private Token Expect(TokenType type, string message)
    {
        if (Check(type)) return Advance();
        throw new Exception($"Syntax Error [Line {Peek().Line}]: {message}. Found '{Peek().Lexeme}' instead.");
    }

    private bool Match(TokenType type)
{
    if (Check(type))
    {
        Advance();
        return true;
    }
    return false;
}

// 2. STATEMENTS (Return Stmt objects)
  

// program 

  public List<Stmt> Parse()
    {
        List<Stmt> statements = new List<Stmt>();
        
        while (!IsAtEnd())
        {
            statements.Add(ParseStatement());
        }
        
        return statements;
    }

    // router for statements 
    private Stmt ParseStatement()
    {
        
        if (Check(TokenType.Stone) || Check(TokenType.Water) || Check(TokenType.Papyrus) || Check(TokenType.Maat))
            return ParseVariableDeclaration();
        if (Check(TokenType.Judge)) return ParseIfStatement();
        if (Check(TokenType.Flow)) return ParseWhileStatement();
        if (Check(TokenType.Carve)) return ParsePrintStatement();
        if (Check(TokenType.LeftBrace)) return ParseBlock();
        if (Check(TokenType.Identifier)) return ParseAssignmentStatement();
        if (Check(TokenType.Shatter)) return ParseBreakStatement();
        if (Check(TokenType.Persist)) return ParseContinueStatement();
        if (Check(TokenType.Dynasty)) return ParseFunctionDeclaration(); 
        if (Check(TokenType.Tribute)) return ParseReturnStatement();

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
        Advance(); // judge
        Expect(TokenType.LeftParen, "Expected '(' after 'judge'.");
        Expr condition = ParseExpression();
        Expect(TokenType.RightParen, "Expected ')' after condition.");
        
        Stmt thenBranch = ParseBlock();

        Stmt elseBranch = null;
          
        // optional 
        if (Check(TokenType.Banish))
        {
            Advance(); // banish
            elseBranch = ParseBlock();
        }

        return new IfStmt(condition, thenBranch, elseBranch);
    }

    private Stmt ParseWhileStatement()
    {
        Advance(); // flow
        Expect(TokenType.LeftParen, "Expected '(' after 'flow'.");
        Expr condition = ParseExpression();
        Expect(TokenType.RightParen, "Expected ')' after condition.");
        Stmt body = ParseBlock();
        return new WhileStmt(condition, body);
    }

    private Stmt ParsePrintStatement()
    {
        Advance(); // carve
        Expect(TokenType.LeftParen, "Expected '(' after 'carve'.");
        Expr value = ParseExpression();
        Expect(TokenType.RightParen, "Expected ')' after value.");
        Expect(TokenType.Semicolon, "Expected ';'.");
        return new PrintStmt(value);
    }



    private Stmt ParseFunctionDeclaration()
{
    Advance(); // dynasty
    Token name = Expect(TokenType.Identifier, "Expected function name.");
    Expect(TokenType.LeftParen, "Expected '(' after function name.");

    List<Token> paramTypes = new List<Token>();
    List<Token> parameters = new List<Token>();

    // Check if there are any parameters to parse
    if (!Check(TokenType.RightParen)) 
    {
        do
        {
            // 1. Check for a valid type (stone, water, papyrus, maat)

            if (!(Check(TokenType.Stone) || Check(TokenType.Water) || 
                  Check(TokenType.Papyrus) || Check(TokenType.Maat)))
            {
                throw new Exception($"Syntax Error [Line {Peek().Line}]: Expected a valid parameter type. Found '{Peek().Lexeme}'.");
            }

            // 2. Capture the metadata for the Symbol Table

            paramTypes.Add(Advance()); 
            parameters.Add(Expect(TokenType.Identifier, "Expected parameter name."));

            // 3. If there's a comma, we MUST have another parameter
        } while (Match(TokenType.Comma)); 
    }

    Expect(TokenType.RightParen, "Expected ')' after parameters.");

    if (!Check(TokenType.LeftBrace)) 
        throw new Exception($"Syntax Error [Line {Peek().Line}]: Expected '{{' before function body.");

    // Functions must have a block body for Scope Resolution
    BlockStmt body = (BlockStmt)ParseBlock(); 

    return new FunctionStmt(name, parameters, paramTypes, body);
}

    private Stmt ParseReturnStatement() 
    {
        Token keyword = Advance(); // tribute
        Expr value = null; 
        
        // If there is an expression before the semicolon, parse it
        if (!Check(TokenType.Semicolon))
        {
            value = ParseExpression();
        }
        
        Expect(TokenType.Semicolon, "Expected ';' after return value.");
        return new ReturnStmt(keyword, value);
    }



    // 3. EXPRESSIONS (Return Expr objects)
  

    private Expr ParseExpression() => ParseLogicOr();

    private Expr ParseLogicOr()
    {
        Expr expr = ParseLogicAnd(); 
        while (Check(TokenType.Or)) 
        {
            Token op = Advance(); // or 
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
            expr = new BinaryExpr(expr, op, right); 
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
        return ParseCall();
    }

    private Expr ParseCall()
    {
        Expr expr = ParsePrimary(); // Read the function name

        // Check if there is a parenthesis immediately after the name
        while (Check(TokenType.LeftParen))
        {
            Token paren = Advance(); // Consume (
            List<Expr> arguments = new List<Expr>();
            
        
            if (!Check(TokenType.RightParen))
            {
                do
                {
                    arguments.Add(ParseExpression());
                    if (Check(TokenType.Comma)) Advance(); 
                    else break; 
                } while (true);
            }
            
            Expect(TokenType.RightParen, "Expected ')' after arguments.");
            expr = new CallExpr(expr, paren, arguments);
        }
        
        return expr;
    }

    private Expr ParsePrimary()
    {
        if (Check(TokenType.True)) return new LiteralExpr(Advance(), true); 
        if (Check(TokenType.False)) return new LiteralExpr(Advance(), false);
        if (Check(TokenType.Integer))
        {
            Token token = Advance();
            
            // token , value 
            return new LiteralExpr(token, int.Parse(token.Lexeme));
        }
        if (Check(TokenType.Float))
        {
            Token token = Advance();
            return new LiteralExpr(token, double.Parse(token.Lexeme));
        }

        if (Check(TokenType.StringLiteral))
        {
            Token token = Advance();
            // We strip the quotes and pass the string value
            string value = token.Lexeme.Substring(1, token.Lexeme.Length - 2);
            return new LiteralExpr(token, value);
        }

        if (Check(TokenType.Identifier))
        {
            return new VariableExpr(Advance());
        }
        if (Check(TokenType.LeftParen))
        {
            Advance();
            Expr expr = ParseExpression();
            Expect(TokenType.RightParen, "Expected ')' after expression.");
            return expr;
        }

        throw new Exception($"Syntax Error: Expected a value. Found '{Peek().Lexeme}'.");
    }

    private Stmt ParseBreakStatement()
    {
        Token keyword = Advance(); // Consume shatter
        Expect(TokenType.Semicolon, "Expected ';' after shatter statement.");
        return new BreakStmt(keyword);
    }

    private Stmt ParseContinueStatement()
    {
        Token keyword = Advance(); // Consume 'persist'
        Expect(TokenType.Semicolon, "Expected ';' after persist statement.");
        return new ContinueStmt(keyword);
    }
}

