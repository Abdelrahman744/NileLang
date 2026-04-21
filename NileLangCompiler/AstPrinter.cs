  
using System;
using System.Collections.Generic;

namespace NileLangCompiler;

public static class AstPrinter
{
    public static void PrintTree(List<Stmt> statements)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[ Abstract Syntax Tree ]");
        Console.ResetColor();
        
        foreach (var stmt in statements)
        {
            PrintStatement(stmt, "");
        }
    }

    private static void PrintStatement(Stmt stmt, string indent)
    {
        if (stmt is VarDeclStmt v) {
            Console.WriteLine($"{indent}└── VarDecl: {v.TypeKeyword.Lexeme} {v.Name.Lexeme} = {FormatExpr(v.Initializer)}");
        } 
        else if (stmt is AssignStmt a) {
            Console.WriteLine($"{indent}└── Assign: {a.Name.Lexeme} = {FormatExpr(a.Value)}");
        } 
        else if (stmt is PrintStmt p) {
            Console.WriteLine($"{indent}└── Carve (Print): {FormatExpr(p.Expression)}");
        } 
        else if (stmt is BlockStmt b) {
            Console.WriteLine($"{indent}└── Block {{");
            foreach (var s in b.Statements) {
                PrintStatement(s, indent + "    ");
            }
            Console.WriteLine($"{indent}    }}");
        } 
        else if (stmt is IfStmt i) {
            Console.WriteLine($"{indent}└── Judge (If): Condition {FormatExpr(i.Condition)}");
            Console.WriteLine($"{indent}    Then:");
            PrintStatement(i.ThenBranch, indent + "    ");
            if (i.ElseBranch != null) {
                Console.WriteLine($"{indent}    Banish (Else):");
                PrintStatement(i.ElseBranch, indent + "    ");
            }
        } 
        else if (stmt is WhileStmt w) {
            Console.WriteLine($"{indent}└── Flow (While): Condition {FormatExpr(w.Condition)}");
            Console.WriteLine($"{indent}    Do:");
            PrintStatement(w.Body, indent + "    ");
        }else if (stmt is BreakStmt) Console.WriteLine($"{indent}└── Shatter (Break)");
        else if (stmt is ContinueStmt) Console.WriteLine($"{indent}└── Persist (Continue)");
        else if (stmt is FunctionStmt) Console.WriteLine($"{indent}└── Dynasty (Function): {((FunctionStmt)stmt).Name.Lexeme}");
        else if (stmt is ReturnStmt) Console.WriteLine($"{indent}└── Tribute (Return): {FormatExpr(((ReturnStmt)stmt).Value)}");

    }

    // Formats math and logic into LISP-like nested text: e.g., (+ 10 (* 5 2))
    private static string FormatExpr(Expr expr)
    {
        if (expr is BinaryExpr b) return $"({b.Operator.Lexeme} {FormatExpr(b.Left)} {FormatExpr(b.Right)})";
        if (expr is LogicalExpr l) return $"({l.Operator.Lexeme} {FormatExpr(l.Left)} {FormatExpr(l.Right)})";
        if (expr is UnaryExpr u) return $"({u.Operator.Lexeme} {FormatExpr(u.Right)})";
        if (expr is LiteralExpr lit) return lit.Value?.ToString() ?? "null";
        if (expr is VariableExpr varExpr) return varExpr.Name.Lexeme;
        if (expr is CallExpr c) return $"{((VariableExpr)c.Callee).Name.Lexeme}(...)";
        return "UnknownExpr";
    }
}