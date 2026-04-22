using System;
using System.Collections.Generic;

namespace NileLangCompiler;

public class SemanticAnalyzer
{
    private SymbolTable _environment = new SymbolTable(); 
    private int _loopDepth = 0; // Tracks if we are inside a flow loop 

    public void Analyze(List<Stmt> statements)
    {
        foreach (var stmt in statements)
        {
            CheckStatement(stmt);
        }
    }

    
    // 1. CHECK STATEMENTS (No execution, just validation)
    
    private void CheckStatement(Stmt stmt)
    {
        if (stmt is VarDeclStmt v)
        {
            TokenType valueType = CheckExpression(v.Initializer);
            CheckTypeMatch(v.Name.Lexeme, v.TypeKeyword.Type, valueType);

            // Declare it in the symbol table with its TYPE, but NO VALUE!
            IdentifierInfo newRow = new IdentifierInfo(v.Name.Lexeme, v.TypeKeyword.Type, v.Name.Line, null);
            _environment.Declare(newRow);
        }
        else if (stmt is AssignStmt a)
        {
            TokenType valueType = CheckExpression(a.Value);
            IdentifierInfo variable = _environment.Lookup(a.Name.Lexeme); // Throws error if undeclared
            CheckTypeMatch(a.Name.Lexeme, variable.Type, valueType);
        }
        else if (stmt is PrintStmt p)
        {
            CheckExpression(p.Expression); 
        }
        else if (stmt is BlockStmt b)
        {
            _environment.PushScope();
            foreach (var statement in b.Statements) CheckStatement(statement);
            _environment.PopScope();
        }
        else if (stmt is IfStmt ifStmt) // FIX: Changed 'i' to 'ifStmt'
        {
            CheckExpression(ifStmt.Condition);
            CheckStatement(ifStmt.ThenBranch);
            if (ifStmt.ElseBranch != null) CheckStatement(ifStmt.ElseBranch);
        }
        else if (stmt is WhileStmt w)
        {
            CheckExpression(w.Condition);
            
            _loopDepth++;
            CheckStatement(w.Body); // Check the body EXACTLY ONCE. Do not loop!
            _loopDepth--;
        }
        else if (stmt is BreakStmt brk)
        {
            if (_loopDepth == 0) throw new Exception($"Semantic Error: Invalid Control Flow. '{brk.Keyword.Lexeme}' outside flow loop.");
        }
        else if (stmt is ContinueStmt cont)
        {
            if (_loopDepth == 0) throw new Exception($"Semantic Error: Invalid Control Flow. '{cont.Keyword.Lexeme}' outside flow loop.");
        }
        else if (stmt is FunctionStmt f)
        {
            IdentifierInfo info = new IdentifierInfo(f.Name.Lexeme, TokenType.Dynasty, f.Name.Line, f);
            _environment.Declare(info);

            _environment.PushScope();
            for (int i = 0; i < f.Parameters.Count; i++)
            {
                IdentifierInfo paramInfo = new IdentifierInfo(f.Parameters[i].Lexeme, f.ParamTypes[i].Type, f.Parameters[i].Line, null);
                _environment.Declare(paramInfo);
            }
            CheckStatement(f.Body); 
            _environment.PopScope();
        }
        else if (stmt is ReturnStmt r)
        {
            if (r.Value != null) CheckExpression(r.Value);
        }
    }

    // ==========================================
    // 2. CHECK EXPRESSIONS (Determine Data Types)
    // ==========================================
    private TokenType CheckExpression(Expr expr)
    {
        if (expr is LiteralExpr l)
        {
            if (l.Value is double || l.Value is int) return TokenType.Stone;
            if (l.Value is string) return TokenType.Papyrus;
            if (l.Value is bool) return TokenType.Maat;
            return TokenType.Unknown;
        }
        if (expr is VariableExpr v)
        {
            return _environment.Lookup(v.Name.Lexeme).Type;
        }
        if (expr is LogicalExpr log)
        {
            CheckExpression(log.Left);
            CheckExpression(log.Right);
            return TokenType.Maat; 
        }
        if (expr is UnaryExpr u)
        {
            return CheckExpression(u.Right);
        }
        if (expr is BinaryExpr b)
        {
            TokenType left = CheckExpression(b.Left);
            TokenType right = CheckExpression(b.Right);

            // GUARDRAIL: Prevent adding strings to integers!
            if (b.Operator.Type == TokenType.Plus)
            {
                if (left != right) throw new Exception($"Semantic Error: Type Mismatch. Cannot add {left} to {right}.");
            }
            else if (b.Operator.Type == TokenType.Minus || b.Operator.Type == TokenType.Multiply || b.Operator.Type == TokenType.Divide)
            {
                if ((left != TokenType.Stone && left != TokenType.Water) || 
                    (right != TokenType.Stone && right != TokenType.Water))
                {
                    throw new Exception($"Semantic Error: Math operation '{b.Operator.Lexeme}' requires numbers.");
                }
            }

            // Relational operators return boolean
            if (b.Operator.Type == TokenType.Equals || b.Operator.Type == TokenType.NotEquals || 
                b.Operator.Type == TokenType.GreaterThan || b.Operator.Type == TokenType.LessThan)
            {
                return TokenType.Maat;
            }

            return TokenType.Stone;
        }
        if (expr is CallExpr c)
        {
            IdentifierInfo callee = _environment.Lookup(((VariableExpr)c.Callee).Name.Lexeme); 
            if (!(callee.Value is FunctionStmt function)) throw new Exception("Semantic Error: Can only call defined functions.");

            if (c.Arguments.Count != function.Parameters.Count)
                throw new Exception($"Semantic Error: '{function.Name.Lexeme}' expects {function.Parameters.Count} arguments, but got {c.Arguments.Count}.");

            for (int i = 0; i < c.Arguments.Count; i++)
            {
                TokenType argType = CheckExpression(c.Arguments[i]);
                TokenType paramType = function.ParamTypes[i].Type;
                CheckTypeMatch(function.Name.Lexeme, paramType, argType);
            }
            
            return TokenType.Stone;
        }
        
        throw new Exception("Unknown expression type.");
    }

    // ==========================================
    // 3. HELPERS
    // ==========================================
    private void CheckTypeMatch(string context, TokenType expected, TokenType actual)
    {
        if ((expected == TokenType.Stone || expected == TokenType.Water) && 
            (actual == TokenType.Stone || actual == TokenType.Water)) return;
            
        if (expected == actual) return;

        throw new Exception($"Semantic Error: Type Mismatch in '{context}'. Expected {expected}, got {actual}.");
    }
}