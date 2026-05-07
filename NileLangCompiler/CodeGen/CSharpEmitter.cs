using System;
using System.Collections.Generic;
using System.Text;

namespace NileLangCompiler;

public class CSharpEmitter
{
    private StringBuilder _sb = new StringBuilder();
    private int _indentLevel = 0;

    // ============================================================
    //  PUBLIC API
    // ============================================================

    /// <summary>
    /// Takes the full AST and produces a complete, compilable C# source file.
    /// Dynasty functions become static methods; everything else goes into Main().
    /// </summary>
    public string Emit(List<Stmt> statements)
    {
        // Separate functions from top-level statements
        List<FunctionStmt> functions = new List<FunctionStmt>();
        List<Stmt> mainBody = new List<Stmt>();

        foreach (var stmt in statements)
        {
            if (stmt is FunctionStmt f)
                functions.Add(f);
            else
                mainBody.Add(stmt);
        }

        // Build the C# source
        AppendLine("using System;");
        AppendLine("");
        AppendLine("class NileLangProgram");
        AppendLine("{");
        _indentLevel++;

        // Emit all dynasty functions as static methods
        foreach (var func in functions)
        {
            EmitFunction(func);
            AppendLine("");
        }

        // Emit Main() with all top-level statements
        AppendLine("static void Main()");
        AppendLine("{");
        _indentLevel++;

        foreach (var stmt in mainBody)
        {
            EmitStatement(stmt);
        }

        _indentLevel--;
        AppendLine("}");

        _indentLevel--;
        AppendLine("}");

        return _sb.ToString();
    }

    // ============================================================
    //  STATEMENT EMISSION
    // ============================================================

    private void EmitStatement(Stmt stmt)
    {
        if (stmt is VarDeclStmt v) EmitVarDecl(v);
        else if (stmt is AssignStmt a) EmitAssign(a);
        else if (stmt is PrintStmt p) EmitPrint(p);
        else if (stmt is IfStmt i) EmitIf(i);
        else if (stmt is WhileStmt w) EmitWhile(w);
        else if (stmt is BlockStmt b) EmitBlock(b);
        else if (stmt is BreakStmt) AppendLine("break;");
        else if (stmt is ContinueStmt) AppendLine("continue;");
        else if (stmt is FunctionStmt f) EmitFunction(f);
        else if (stmt is ReturnStmt r) EmitReturn(r);
    }

    private void EmitVarDecl(VarDeclStmt stmt)
    {
        string csType = MapType(stmt.TypeKeyword.Type);
        string expr = EmitExpression(stmt.Initializer);
        AppendLine($"{csType} {stmt.Name.Lexeme} = {expr};");
    }

    private void EmitAssign(AssignStmt stmt)
    {
        string expr = EmitExpression(stmt.Value);
        AppendLine($"{stmt.Name.Lexeme} = {expr};");
    }

    private void EmitPrint(PrintStmt stmt)
    {
        string expr = EmitExpression(stmt.Expression);
        AppendLine($"Console.WriteLine({expr});");
    }

    private void EmitIf(IfStmt stmt)
    {
        string condition = EmitExpression(stmt.Condition);
        AppendLine($"if ({condition})");
        EmitBlockBody(stmt.ThenBranch);

        if (stmt.ElseBranch != null)
        {
            AppendLine("else");
            EmitBlockBody(stmt.ElseBranch);
        }
    }

    private void EmitWhile(WhileStmt stmt)
    {
        string condition = EmitExpression(stmt.Condition);
        AppendLine($"while ({condition})");
        EmitBlockBody(stmt.Body);
    }

    private void EmitBlock(BlockStmt stmt)
    {
        AppendLine("{");
        _indentLevel++;
        foreach (var s in stmt.Statements)
        {
            EmitStatement(s);
        }
        _indentLevel--;
        AppendLine("}");
    }

    /// <summary>
    /// Emits the body of an if/while/etc. If it's a BlockStmt, emit braces + contents.
    /// Otherwise wrap the single statement in braces.
    /// </summary>
    private void EmitBlockBody(Stmt body)
    {
        if (body is BlockStmt block)
        {
            EmitBlock(block);
        }
        else
        {
            AppendLine("{");
            _indentLevel++;
            EmitStatement(body);
            _indentLevel--;
            AppendLine("}");
        }
    }

    private void EmitFunction(FunctionStmt stmt)
    {
        string returnType = InferReturnType(stmt);
        
        // Build parameter list: "int a, int b"
        List<string> paramParts = new List<string>();
        for (int i = 0; i < stmt.Parameters.Count; i++)
        {
            string pType = MapType(stmt.ParamTypes[i].Type);
            string pName = stmt.Parameters[i].Lexeme;
            paramParts.Add($"{pType} {pName}");
        }
        string paramList = string.Join(", ", paramParts);

        AppendLine($"static {returnType} {stmt.Name.Lexeme}({paramList})");
        EmitBlock(stmt.Body);
    }

    private void EmitReturn(ReturnStmt stmt)
    {
        if (stmt.Value != null)
        {
            string expr = EmitExpression(stmt.Value);
            AppendLine($"return {expr};");
        }
        else
        {
            AppendLine("return;");
        }
    }

    // ============================================================
    //  EXPRESSION EMISSION
    // ============================================================

    private string EmitExpression(Expr expr)
    {
        if (expr is LiteralExpr lit) return EmitLiteral(lit);
        if (expr is VariableExpr v) return v.Name.Lexeme;
        if (expr is BinaryExpr b) return EmitBinary(b);
        if (expr is LogicalExpr log) return EmitLogical(log);
        if (expr is UnaryExpr u) return EmitUnary(u);
        if (expr is CallExpr c) return EmitCall(c);

        return "/* unknown expr */";
    }

    private string EmitLiteral(LiteralExpr expr)
    {
        if (expr.Value is bool boolVal)
            return boolVal ? "true" : "false";

        if (expr.Value is int intVal)
            return intVal.ToString();

        if (expr.Value is double doubleVal)
            return doubleVal.ToString();

        if (expr.Value is string strVal)
            return $"\"{EscapeString(strVal)}\"";

        return "null";
    }

    private string EmitBinary(BinaryExpr expr)
    {
        string left = EmitExpression(expr.Left);
        string op = expr.Operator.Lexeme;
        string right = EmitExpression(expr.Right);
        return $"({left} {op} {right})";
    }

    private string EmitLogical(LogicalExpr expr)
    {
        string left = EmitExpression(expr.Left);
        string op = expr.Operator.Lexeme;
        string right = EmitExpression(expr.Right);
        return $"({left} {op} {right})";
    }

    private string EmitUnary(UnaryExpr expr)
    {
        string op = expr.Operator.Lexeme;
        string right = EmitExpression(expr.Right);
        return $"({op}{right})";
    }

    private string EmitCall(CallExpr expr)
    {
        string callee = EmitExpression(expr.Callee);
        List<string> args = new List<string>();
        foreach (var arg in expr.Arguments)
        {
            args.Add(EmitExpression(arg));
        }
        return $"{callee}({string.Join(", ", args)})";
    }

    // ============================================================
    //  HELPERS
    // ============================================================

    /// <summary>
    /// Maps NileLang type tokens to C# type names.
    /// </summary>
    private string MapType(TokenType type)
    {
        return type switch
        {
            TokenType.Stone => "int",
            TokenType.Water => "double",
            TokenType.Papyrus => "string",
            TokenType.Maat => "bool",
            _ => "var"
        };
    }

    /// <summary>
    /// Walks a function body to find the first tribute (return) statement
    /// and infers the C# return type from its expression.
    /// No tribute found → void.
    /// </summary>
    private string InferReturnType(FunctionStmt func)
    {
        ReturnStmt ret = FindReturn(func.Body.Statements);

        if (ret == null || ret.Value == null)
            return "void";

        // Determine the type of the returned expression
        return InferExprType(ret.Value);
    }

    /// <summary>
    /// Recursively searches a list of statements for the first ReturnStmt.
    /// Looks inside blocks, if/else branches, and while bodies.
    /// </summary>
    private ReturnStmt FindReturn(List<Stmt> statements)
    {
        foreach (var stmt in statements)
        {
            if (stmt is ReturnStmt r)
                return r;

            if (stmt is BlockStmt block)
            {
                var found = FindReturn(block.Statements);
                if (found != null) return found;
            }

            if (stmt is IfStmt ifStmt)
            {
                if (ifStmt.ThenBranch is BlockStmt thenBlock)
                {
                    var found = FindReturn(thenBlock.Statements);
                    if (found != null) return found;
                }
                if (ifStmt.ElseBranch is BlockStmt elseBlock)
                {
                    var found = FindReturn(elseBlock.Statements);
                    if (found != null) return found;
                }
            }

            if (stmt is WhileStmt whileStmt)
            {
                if (whileStmt.Body is BlockStmt whileBlock)
                {
                    var found = FindReturn(whileBlock.Statements);
                    if (found != null) return found;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Infers the C# type of an expression based on its AST structure.
    /// This is a simple heuristic — the semantic analyzer already validated types.
    /// </summary>
    private string InferExprType(Expr expr)
    {
        if (expr is LiteralExpr lit)
        {
            if (lit.Value is int) return "int";
            if (lit.Value is double) return "double";
            if (lit.Value is string) return "string";
            if (lit.Value is bool) return "bool";
        }

        // For binary math operations, default to int 
        if (expr is BinaryExpr bin)
        {
            // Comparison/equality operators always return bool
            if (bin.Operator.Type == TokenType.Equals || bin.Operator.Type == TokenType.NotEquals ||
                bin.Operator.Type == TokenType.GreaterThan || bin.Operator.Type == TokenType.LessThan ||
                bin.Operator.Type == TokenType.GreaterOrEqual || bin.Operator.Type == TokenType.LessOrEqual)
                return "bool";

            // Check if either side is a double → result is double
            string leftType = InferExprType(bin.Left);
            string rightType = InferExprType(bin.Right);
            if (leftType == "double" || rightType == "double")
                return "double";

            return "int";
        }

        if (expr is LogicalExpr) return "bool";
        if (expr is UnaryExpr u)
        {
            if (u.Operator.Type == TokenType.Not) return "bool";
            return InferExprType(u.Right);
        }

        // For variables and calls, we can't easily know without the symbol table,
        // so default to int (most common numeric type in NileLang)
        return "int";
    }

    /// <summary>
    /// Escapes special characters in string literals for C# output.
    /// </summary>
    private string EscapeString(string input)
    {
        return input
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    /// <summary>
    /// Appends a line with proper indentation to the output buffer.
    /// </summary>
    private void AppendLine(string line)
    {
        if (string.IsNullOrEmpty(line))
        {
            _sb.AppendLine();
        }
        else
        {
            _sb.Append(new string(' ', _indentLevel * 4));
            _sb.AppendLine(line);
        }
    }
}
