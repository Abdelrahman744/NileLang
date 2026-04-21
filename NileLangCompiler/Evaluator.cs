using System;
using System.Collections.Generic;

namespace NileLangCompiler;

public class Evaluator
{
    private SymbolTable _environment = new SymbolTable();
    private int _loopDepth = 0; // Tracks if we are inside a flow loop

    public void Interpret(List<Stmt> statements)
    {
        Console.WriteLine("\n=== NileLang Execution Engine ===");
        try
        {
            foreach (var stmt in statements)
            {
                Execute(stmt);
            }
            Console.WriteLine("=== Execution Finished Successfully ===\n");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Runtime Error: {ex.Message}");
            Console.ResetColor();
        }
    }

    // ==========================================
    // 1. EXECUTE STATEMENTS (Actions)
    // ==========================================
    private void Execute(Stmt stmt)
    {
        if (stmt is VarDeclStmt v)
        {
            object value = Evaluate(v.Initializer);
            IdentifierInfo newRow = new IdentifierInfo(v.Name.Lexeme, v.TypeKeyword.Type, v.Name.Line, value);
            _environment.Declare(newRow);
        }
        else if (stmt is AssignStmt a)
        {
            object value = Evaluate(a.Value);
            _environment.Assign(a.Name.Lexeme, value);
        }
        else if (stmt is PrintStmt p)
        {
            object value = Evaluate(p.Expression);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[Carved]: {value}");
            Console.ResetColor();
        }
        else if (stmt is BlockStmt b)
        {
            _environment.PushScope();
            try
            {
                foreach (var statement in b.Statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                _environment.PopScope();
            }
        }
        else if (stmt is IfStmt i)
        {
            if (IsTruthy(Evaluate(i.Condition)))
            {
                Execute(i.ThenBranch);
            }
            else if (i.ElseBranch != null)
            {
                Execute(i.ElseBranch);
            }
        }
        else if (stmt is WhileStmt w)
        {
            _loopDepth++;
            try
            {
                while (IsTruthy(Evaluate(w.Condition)))
                {
                    try
                    {
                        Execute(w.Body);
                    }
                    catch (ContinueException) { /* persist called */ }
                }
            }
            catch (BreakException) { /* shatter called */ }
            finally
            {
                _loopDepth--;
            }
        }
        else if (stmt is BreakStmt brk)
        {
            if (_loopDepth == 0) throw new Exception($"Semantic Error: Invalid Control Flow. '{brk.Keyword.Lexeme}' can only be used inside a flow loop.");
            throw new BreakException();
        }
        else if (stmt is ContinueStmt cont)
        {
            if (_loopDepth == 0) throw new Exception($"Semantic Error: Invalid Control Flow. '{cont.Keyword.Lexeme}' can only be used inside a flow loop.");
            throw new ContinueException();
        }
        else if (stmt is FunctionStmt f)
        {
            IdentifierInfo info = new IdentifierInfo(f.Name.Lexeme, TokenType.Dynasty, f.Name.Line, f);
            _environment.Declare(info);
        }
        else if (stmt is ReturnStmt r)
        {
            object value = null;
            if (r.Value != null) value = Evaluate(r.Value);
            throw new ReturnException(value); // Jump out of the function!
        }
    }

    // ==========================================
    // 2. EVALUATE EXPRESSIONS (Math, Logic & Functions)
    // ==========================================
    private object Evaluate(Expr expr)
    {
        if (expr is CallExpr c)
        {
            object callee = Evaluate(c.Callee);
            if (!(callee is FunctionStmt function))
            {
                throw new Exception($"Semantic Error: Can only call defined functions.");
            }

            if (c.Arguments.Count != function.Parameters.Count)
            {
                throw new Exception($"Semantic Error: Parameter/Argument Mismatch. '{function.Name.Lexeme}' expects {function.Parameters.Count} arguments, but got {c.Arguments.Count}.");
            }

            List<object> evaluatedArgs = new List<object>();
            foreach (var arg in c.Arguments) evaluatedArgs.Add(Evaluate(arg));

            for (int i = 0; i < evaluatedArgs.Count; i++)
            {
                TokenType expectedType = function.ParamTypes[i].Type;
                CheckArgumentType(function.Name.Lexeme, function.Parameters[i].Lexeme, expectedType, evaluatedArgs[i]);
            }

            _environment.PushScope();
            try
            {
                for (int i = 0; i < function.Parameters.Count; i++)
                {
                    IdentifierInfo paramInfo = new IdentifierInfo(
                        function.Parameters[i].Lexeme,
                        function.ParamTypes[i].Type,
                        function.Parameters[i].Line,
                        evaluatedArgs[i]
                    );
                    _environment.Declare(paramInfo);
                }

                Execute(function.Body);
            }
            catch (ReturnException ret)
            {
                return ret.Value;
            }
            finally
            {
                _environment.PopScope();
            }

            return null;
        }

        if (expr is LiteralExpr l) return l.Value;
        
        if (expr is VariableExpr v)
        {
            IdentifierInfo row = _environment.Lookup(v.Name.Lexeme);
            return row.Value;
        }
        
        if (expr is LogicalExpr log)
        {
            object left = Evaluate(log.Left);
            if (log.Operator.Type == TokenType.Or && IsTruthy(left)) return true;
            if (log.Operator.Type == TokenType.And && !IsTruthy(left)) return false;
            return IsTruthy(Evaluate(log.Right));
        }

        if (expr is UnaryExpr u)
        {
            object right = Evaluate(u.Right);
            if (u.Operator.Type == TokenType.Minus) return -(double)Convert.ToDouble(right);
            if (u.Operator.Type == TokenType.Not) return !IsTruthy(right);
        }

        if (expr is BinaryExpr b)
        {
            object left = Evaluate(b.Left);
            object right = Evaluate(b.Right);

            CheckType(left, right, b.Operator);

            switch (b.Operator.Type)
            {
                case TokenType.Plus:
                    if (left is string || right is string) return left.ToString() + right.ToString();
                    return Convert.ToDouble(left) + Convert.ToDouble(right);
                case TokenType.Minus: return Convert.ToDouble(left) - Convert.ToDouble(right);
                case TokenType.Multiply: return Convert.ToDouble(left) * Convert.ToDouble(right);
                case TokenType.Divide: return Convert.ToDouble(left) / Convert.ToDouble(right);
                case TokenType.GreaterThan: return Convert.ToDouble(left) > Convert.ToDouble(right);
                case TokenType.GreaterOrEqual: return Convert.ToDouble(left) >= Convert.ToDouble(right);
                case TokenType.LessThan: return Convert.ToDouble(left) < Convert.ToDouble(right);
                case TokenType.LessOrEqual: return Convert.ToDouble(left) <= Convert.ToDouble(right);
                case TokenType.Equals: return IsEqual(left, right);
                case TokenType.NotEquals: return !IsEqual(left, right);
            }
        }
        
        throw new Exception("Unknown expression.");
    }

    // ==========================================
    // 3. SEMANTIC CHECKERS & HELPERS
    // ==========================================
    private void CheckType(object left, object right, Token operatorToken)
    {
        if (operatorToken.Type == TokenType.Minus || operatorToken.Type == TokenType.Multiply || 
            operatorToken.Type == TokenType.Divide || operatorToken.Type == TokenType.GreaterThan ||
            operatorToken.Type == TokenType.LessThan)
        {
            if ((left is double || left is int) && (right is double || right is int)) return;
            throw new Exception($"Semantic Error: Type Mismatch. Cannot use '{operatorToken.Lexeme}' on {left.GetType().Name} and {right.GetType().Name}.");
        }

        if (operatorToken.Type == TokenType.Plus)
        {
            if ((left is double || left is int) && (right is double || right is int)) return;
            if (left is string || right is string) return;
            throw new Exception($"Semantic Error: Type Mismatch. Cannot add {left.GetType().Name} to {right.GetType().Name}.");
        }
    }

    private void CheckArgumentType(string funcName, string paramName, TokenType expectedType, object value)
    {
        bool isValid = false;
        if ((expectedType == TokenType.Stone || expectedType == TokenType.Water) && (value is double || value is int)) isValid = true;
        else if (expectedType == TokenType.Papyrus && value is string) isValid = true;
        else if (expectedType == TokenType.Maat && value is bool) isValid = true;

        if (!isValid)
        {
            throw new Exception($"Semantic Error: Parameter/Argument Mismatch in function '{funcName}'. Parameter '{paramName}' expects {expectedType}, but received {value?.GetType().Name ?? "null"}.");
        }
    }

    private bool IsTruthy(object obj)
    {
        if (obj == null) return false;
        if (obj is bool b) return b;
        return true; 
    }

    private bool IsEqual(object a, object b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        if ((a is int || a is double) && (b is int || b is double))
        {
            return Convert.ToDouble(a) == Convert.ToDouble(b);
        }
        return a.Equals(b);
    }
}

// ==========================================
// 4. JUMP EXCEPTIONS
// ==========================================
public class BreakException : Exception { }
public class ContinueException : Exception { }
public class ReturnException : Exception
{
    public object Value { get; }
    public ReturnException(object value) { Value = value; }
}