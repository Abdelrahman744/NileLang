using System;
using System.Collections.Generic;

namespace NileLangCompiler;

public class SymbolTable
{
    // The Map now stores the Metadata row, not just a raw object!
    private Stack<Dictionary<string, IdentifierInfo>> _scopes;

    public SymbolTable()
    {
        _scopes = new Stack<Dictionary<string, IdentifierInfo>>();
        PushScope();
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
            Console.WriteLine("Semantic Warning: Attempt to pop global scope ignored.");
        }
    }

    // Updated to accept the full metadata row
    public void Declare(IdentifierInfo info)
    {
        var currentScope = _scopes.Peek();

        if (currentScope.ContainsKey(info.Name))
        {
            // Now we can provide the exact line number of the error!
            throw new Exception($"Semantic Error: Redeclaration of '{info.Name}' at line {info.DeclaredLine}");
        }

        currentScope[info.Name] = info;
    }

    // Now returns the full metadata row
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

    public void Assign(string name, object newValue)
    {
        foreach (var scope in _scopes)
        {
            if (scope.ContainsKey(name))
            {
                scope[name].Value = newValue;
                return;
            }
        }

        throw new Exception($"Semantic Error: Cannot assign to undeclared variable '{name}'.");
    }
}