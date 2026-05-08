using System;
using System.Collections.Generic;

namespace NileLangCompiler;

/// <summary>
/// Runtime scope chain for the NileLang interpreter.
/// Each environment holds variable values and links to an enclosing (parent) scope.
/// This is separate from the compile-time SymbolTable — it stores actual runtime values.
/// </summary>
public class RuntimeEnvironment
{
    private readonly Dictionary<string, object> _values = new Dictionary<string, object>();
    public RuntimeEnvironment Enclosing { get; }

    /// <summary>
    /// Creates a new environment. Pass null for the global scope,
    /// or an existing environment to create a child scope.
    /// </summary>
    public RuntimeEnvironment(RuntimeEnvironment enclosing = null)
    {
        Enclosing = enclosing;
    }

    /// <summary>
    /// Defines a new variable in the current scope.
    /// </summary>
    public void Define(string name, object value)
    {
        _values[name] = value;
    }

    /// <summary>
    /// Looks up a variable by name, walking up the scope chain.
    /// </summary>
    public object Get(string name)
    {
        if (_values.ContainsKey(name))
            return _values[name];

        if (Enclosing != null)
            return Enclosing.Get(name);

        throw new Exception($"Runtime Error: Undefined variable '{name}'.");
    }

    /// <summary>
    /// Assigns a new value to an existing variable, walking up the scope chain.
    /// </summary>
    public void Assign(string name, object value)
    {
        if (_values.ContainsKey(name))
        {
            _values[name] = value;
            return;
        }

        if (Enclosing != null)
        {
            Enclosing.Assign(name, value);
            return;
        }

        throw new Exception($"Runtime Error: Undefined variable '{name}'.");
    }
}
