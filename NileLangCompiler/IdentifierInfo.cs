namespace NileLangCompiler;

// Class for the row of the table (The metadata of the Identifier)
public class IdentifierInfo
{
    public string Name { get; }
    public TokenType DataType { get; } // e.g., Stone, Water, Papyrus, Maat
    public int DeclaredLine { get; }
    
    // We still store the actual value so the Evaluator can use it later
    public object Value { get; set; }  

    public IdentifierInfo(string name, TokenType dataType, int declaredLine, object value = null)
    {
        Name = name;
        DataType = dataType;
        DeclaredLine = declaredLine;
        Value = value;
    }
}