using System.Collections.Generic;

namespace NileLangCompiler;

// ==========================================
// 1. EXPRESSIONS (Things that produce a value)
// ==========================================
public abstract class Expr { }

public class BinaryExpr : Expr
{
    public Expr Left { get; }
    public Token Operator { get; }
    public Expr Right { get; }

    public BinaryExpr(Expr left, Token operatorToken, Expr right)
    {
        Left = left;
        Operator = operatorToken;
        Right = right;
    }
}

public class LogicalExpr : Expr
{
    public Expr Left { get; }
    public Token Operator { get; }
    public Expr Right { get; }

    public LogicalExpr(Expr left, Token operatorToken, Expr right)
    {
        Left = left;
        Operator = operatorToken;
        Right = right;
    }
}

public class UnaryExpr : Expr
{
    public Token Operator { get; }
    public Expr Right { get; }

    public UnaryExpr(Token operatorToken, Expr right)
    {
        Operator = operatorToken;
        Right = right;
    }
}

public class LiteralExpr : Expr
{
    public object Value { get; }

    public LiteralExpr(object value)
    {
        Value = value;
    }
}

public class VariableExpr : Expr
{
    public Token Name { get; }

    public VariableExpr(Token name)
    {
        Name = name;
    }
}

// ==========================================
// 2. STATEMENTS (Actions and Control Flow)
// ==========================================
public abstract class Stmt { }

public class VarDeclStmt : Stmt
{
    public Token TypeKeyword { get; }
    public Token Name { get; }
    public Expr Initializer { get; }

    public VarDeclStmt(Token typeKeyword, Token name, Expr initializer)
    {
        TypeKeyword = typeKeyword;
        Name = name;
        Initializer = initializer;
    }
}

public class AssignStmt : Stmt
{
    public Token Name { get; }
    public Expr Value { get; }

    public AssignStmt(Token name, Expr value)
    {
        Name = name;
        Value = value;
    }
}

public class PrintStmt : Stmt
{
    public Expr Expression { get; }

    public PrintStmt(Expr expression)
    {
        Expression = expression;
    }
}

public class BlockStmt : Stmt
{
    public List<Stmt> Statements { get; }

    public BlockStmt(List<Stmt> statements)
    {
        Statements = statements;
    }
}

public class IfStmt : Stmt
{
    public Expr Condition { get; }
    public Stmt ThenBranch { get; }
    public Stmt ElseBranch { get; } // Can be null

    public IfStmt(Expr condition, Stmt thenBranch, Stmt elseBranch)
    {
        Condition = condition;
        ThenBranch = thenBranch;
        ElseBranch = elseBranch;
    }
}

public class WhileStmt : Stmt
{
    public Expr Condition { get; }
    public Stmt Body { get; }

    public WhileStmt(Expr condition, Stmt body)
    {
        Condition = condition;
        Body = body;
    }
}