namespace NileLangCompiler;

public class IdentifierInfo
{
    public string Name { get; }
    public TokenType Type { get; } 
    public int Line { get; }
    public object Value { get; set; }

    public IdentifierInfo(string name, TokenType type, int line, object value)
    {
        Name = name;
        Type = type;
        Line = line;
        Value = value;
    }
}