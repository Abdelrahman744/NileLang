using System;
using System.Collections.Generic;

namespace NileLangCompiler;

public class SymbolTable
{
    private Stack<Dictionary<string, IdentifierInfo>> _scopes = new Stack<Dictionary<string, IdentifierInfo>>();

    public SymbolTable()
    {
        // Global scope
        _scopes.Push(new Dictionary<string, IdentifierInfo>());
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
            // Uses .Line to report exactly where the duplicate was found
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