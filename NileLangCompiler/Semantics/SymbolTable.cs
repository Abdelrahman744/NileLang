using System;
using System.Collections.Generic;

namespace NileLangCompiler;

/// <summary>
/// Scope-aware symbol table using a stack of dictionaries.
/// Each scope maps variable names to their metadata.
/// </summary>
public class SymbolTable
{
    private Stack<Dictionary<string, IdentifierInfo>> _scopes = new Stack<Dictionary<string, IdentifierInfo>>();

    public SymbolTable()
    {
        _scopes.Push(new Dictionary<string, IdentifierInfo>()); // Global scope
    }

    public void PushScope()
    {
        _scopes.Push(new Dictionary<string, IdentifierInfo>());
    }

    public void PopScope()
    {
        if (_scopes.Count > 1)
        {
            _scopes.Pop();
        }
        else
        {
            throw new Exception("Compiler Error: Cannot pop global scope.");
        }
    }

    public void Declare(IdentifierInfo info)
    {
        var currentScope = _scopes.Peek();
        if (currentScope.ContainsKey(info.Name))
        {
            IdentifierInfo existingInfo = currentScope[info.Name];
            throw new Exception($"Semantic Error: Variable '{info.Name}' was already declared in this scope on line {existingInfo.Line}.");
        }
        currentScope[info.Name] = info;
    }

    public void Assign(string name, object value)
    {
        foreach (var scope in _scopes)
        {
            if (scope.ContainsKey(name))
            {
                scope[name].Value = value;
                return;
            }
        }
        throw new Exception($"Semantic Error: Variable '{name}' has not been declared.");
    }

    public IdentifierInfo Lookup(string name)
    {
        foreach (var scope in _scopes)
        {
            if (scope.ContainsKey(name))
            {
                return scope[name];
            }
        }
        throw new Exception($"Semantic Error: Variable '{name}' has not been declared.");
    }
}
