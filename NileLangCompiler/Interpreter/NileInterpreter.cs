using System;
using System.Collections.Generic;

namespace NileLangCompiler;

// ═════════════════════════════════════════════════════════════════
//  CONTROL FLOW SIGNALS
//  Tree-walking interpreters use lightweight exceptions to unwind
//  the call stack for break, continue, and return.
// ═════════════════════════════════════════════════════════════════

/// <summary>Signal thrown when a "shatter" (break) statement is hit.</summary>
public class BreakSignal : Exception { }

/// <summary>Signal thrown when a "persist" (continue) statement is hit.</summary>
public class ContinueSignal : Exception { }

/// <summary>Signal thrown when a "tribute" (return) statement is hit. Carries the return value.</summary>
public class ReturnSignal : Exception
{
    public object Value { get; }
    public ReturnSignal(object value) { Value = value; }
}


// ═════════════════════════════════════════════════════════════════
//  NILE INTERPRETER — Phase 5 of the compiler pipeline
//  Walks the AST and executes every node in-memory.
// ═════════════════════════════════════════════════════════════════

public class NileInterpreter
{
    private RuntimeEnvironment _globals;
    private RuntimeEnvironment _environment;

    public NileInterpreter()
    {
        _globals = new RuntimeEnvironment();
        _environment = _globals;
    }

    // ============================================================
    //  PUBLIC API
    // ============================================================

    /// <summary>
    /// Executes a full NileLang program (list of top-level statements).
    /// </summary>
    public void Execute(List<Stmt> statements)
    {
        foreach (var stmt in statements)
        {
            ExecuteStatement(stmt);
        }
    }

    // ============================================================
    //  STATEMENT EXECUTION
    // ============================================================

    private void ExecuteStatement(Stmt stmt)
    {
        if (stmt is VarDeclStmt v)         ExecuteVarDecl(v);
        else if (stmt is AssignStmt a)     ExecuteAssign(a);
        else if (stmt is PrintStmt p)      ExecutePrint(p);
        else if (stmt is IfStmt i)         ExecuteIf(i);
        else if (stmt is WhileStmt w)      ExecuteWhile(w);
        else if (stmt is BlockStmt b)      ExecuteBlock(b.Statements, new RuntimeEnvironment(_environment));
        else if (stmt is BreakStmt)        throw new BreakSignal();
        else if (stmt is ContinueStmt)     throw new ContinueSignal();
        else if (stmt is FunctionStmt f)   ExecuteFunctionDecl(f);
        else if (stmt is ReturnStmt r)     ExecuteReturn(r);
    }

    private void ExecuteVarDecl(VarDeclStmt stmt)
    {
        object value = Evaluate(stmt.Initializer);
        _environment.Define(stmt.Name.Lexeme, value);
    }

    private void ExecuteAssign(AssignStmt stmt)
    {
        object value = Evaluate(stmt.Value);
        _environment.Assign(stmt.Name.Lexeme, value);
    }

    private void ExecutePrint(PrintStmt stmt)
    {
        object value = Evaluate(stmt.Expression);
        Console.WriteLine(Stringify(value));
    }

    private void ExecuteIf(IfStmt stmt)
    {
        object condition = Evaluate(stmt.Condition);

        if (IsTruthy(condition))
        {
            ExecuteStatement(stmt.ThenBranch);
        }
        else if (stmt.ElseBranch != null)
        {
            ExecuteStatement(stmt.ElseBranch);
        }
    }

    private void ExecuteWhile(WhileStmt stmt)
    {
        while (IsTruthy(Evaluate(stmt.Condition)))
        {
            try
            {
                ExecuteStatement(stmt.Body);
            }
            catch (BreakSignal)
            {
                break; // shatter → exit the loop
            }
            catch (ContinueSignal)
            {
                continue; // persist → skip to next iteration
            }
        }
    }

    /// <summary>
    /// Executes a list of statements in the given environment scope.
    /// </summary>
    public void ExecuteBlock(List<Stmt> statements, RuntimeEnvironment localEnv)
    {
        RuntimeEnvironment previous = _environment;
        try
        {
            _environment = localEnv;
            foreach (var stmt in statements)
            {
                ExecuteStatement(stmt);
            }
        }
        finally
        {
            _environment = previous;
        }
    }

    private void ExecuteFunctionDecl(FunctionStmt stmt)
    {
        // Store the function definition in the environment
        // We store the AST node itself — it will be called later
        _environment.Define(stmt.Name.Lexeme, stmt);
    }

    private void ExecuteReturn(ReturnStmt stmt)
    {
        object value = null;
        if (stmt.Value != null)
        {
            value = Evaluate(stmt.Value);
        }
        throw new ReturnSignal(value);
    }

    // ============================================================
    //  EXPRESSION EVALUATION
    // ============================================================

    private object Evaluate(Expr expr)
    {
        if (expr is LiteralExpr lit)   return lit.Value;
        if (expr is VariableExpr v)    return _environment.Get(v.Name.Lexeme);
        if (expr is BinaryExpr b)      return EvaluateBinary(b);
        if (expr is LogicalExpr log)   return EvaluateLogical(log);
        if (expr is UnaryExpr u)       return EvaluateUnary(u);
        if (expr is CallExpr c)        return EvaluateCall(c);

        throw new Exception("Runtime Error: Unknown expression type.");
    }

    private object EvaluateBinary(BinaryExpr expr)
    {
        object left = Evaluate(expr.Left);
        object right = Evaluate(expr.Right);

        switch (expr.Operator.Type)
        {
            // ── Arithmetic ──
            case TokenType.Plus:
                if (left is int li && right is int ri)       return li + ri;
                if (left is double ld && right is double rd) return ld + rd;
                if (left is int li2 && right is double rd2)  return li2 + rd2;
                if (left is double ld2 && right is int ri2)  return ld2 + ri2;
                if (left is string ls && right is string rs) return ls + rs;
                // Allow string + anything (auto-stringify)
                if (left is string)  return left.ToString() + Stringify(right);
                if (right is string) return Stringify(left) + right.ToString();
                break;

            case TokenType.Minus:
                return ToDouble(left) - ToDouble(right);

            case TokenType.Multiply:
                if (left is int mli && right is int mri) return mli * mri;
                return ToDouble(left) * ToDouble(right);

            case TokenType.Divide:
                double divisor = ToDouble(right);
                if (divisor == 0) throw new Exception("Runtime Error: Division by zero.");
                if (left is int dli && right is int dri) return dli / dri;
                return ToDouble(left) / divisor;

            // ── Comparison ──
            case TokenType.GreaterThan:    return ToDouble(left) > ToDouble(right);
            case TokenType.GreaterOrEqual: return ToDouble(left) >= ToDouble(right);
            case TokenType.LessThan:       return ToDouble(left) < ToDouble(right);
            case TokenType.LessOrEqual:    return ToDouble(left) <= ToDouble(right);

            // ── Equality ──
            case TokenType.Equals:
                return IsEqual(left, right);
            case TokenType.NotEquals:
                return !IsEqual(left, right);
        }

        throw new Exception($"Runtime Error: Unknown operator '{expr.Operator.Lexeme}'.");
    }

    private object EvaluateLogical(LogicalExpr expr)
    {
        object left = Evaluate(expr.Left);

        // Short-circuit evaluation
        if (expr.Operator.Type == TokenType.Or)
        {
            if (IsTruthy(left)) return left;
        }
        else // And (&&)
        {
            if (!IsTruthy(left)) return left;
        }

        return Evaluate(expr.Right);
    }

    private object EvaluateUnary(UnaryExpr expr)
    {
        object right = Evaluate(expr.Right);

        switch (expr.Operator.Type)
        {
            case TokenType.Minus:
                if (right is int i) return -i;
                return -ToDouble(right);

            case TokenType.Not:
                return !IsTruthy(right);
        }

        throw new Exception($"Runtime Error: Unknown unary operator '{expr.Operator.Lexeme}'.");
    }

    private object EvaluateCall(CallExpr expr)
    {
        // Get the function name
        object callee = Evaluate(expr.Callee);

        if (callee is not FunctionStmt function)
        {
            throw new Exception("Runtime Error: Can only call dynasty functions.");
        }

        // Evaluate all arguments
        List<object> arguments = new List<object>();
        foreach (var arg in expr.Arguments)
        {
            arguments.Add(Evaluate(arg));
        }

        // Check arity
        if (arguments.Count != function.Parameters.Count)
        {
            throw new Exception(
                $"Runtime Error: '{function.Name.Lexeme}' expects {function.Parameters.Count} arguments but got {arguments.Count}.");
        }

        // Create a new scope for the function body, with globals as parent
        RuntimeEnvironment funcEnv = new RuntimeEnvironment(_globals);

        // Bind parameters to argument values
        for (int i = 0; i < function.Parameters.Count; i++)
        {
            funcEnv.Define(function.Parameters[i].Lexeme, arguments[i]);
        }

        // Execute the function body and catch the return signal
        try
        {
            ExecuteBlock(function.Body.Statements, funcEnv);
        }
        catch (ReturnSignal signal)
        {
            return signal.Value;
        }

        return null; // void functions return null
    }

    // ============================================================
    //  HELPERS
    // ============================================================

    /// <summary>
    /// Determines if a value is "truthy" in NileLang.
    /// null → false, false → false, 0 → false, everything else → true.
    /// </summary>
    private bool IsTruthy(object value)
    {
        if (value == null) return false;
        if (value is bool b) return b;
        if (value is int i) return i != 0;
        if (value is double d) return d != 0;
        return true;
    }

    /// <summary>
    /// Checks equality between two NileLang values.
    /// </summary>
    private bool IsEqual(object a, object b)
    {
        if (a == null && b == null) return true;
        if (a == null) return false;
        // Handle int/double comparison
        if (a is int ai && b is double bd) return ai == bd;
        if (a is double ad && b is int bi) return ad == bi;
        return a.Equals(b);
    }

    /// <summary>
    /// Converts a value to double for numeric operations.
    /// </summary>
    private double ToDouble(object value)
    {
        if (value is int i) return i;
        if (value is double d) return d;
        throw new Exception($"Runtime Error: Expected a number, got '{Stringify(value)}'.");
    }

    /// <summary>
    /// Converts any NileLang value to its printable string form.
    /// </summary>
    private string Stringify(object value)
    {
        if (value == null) return "null";
        if (value is bool b) return b ? "True" : "False";
        return value.ToString();
    }
}
