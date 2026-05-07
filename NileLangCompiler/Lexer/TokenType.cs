namespace NileLangCompiler;

/// <summary>
/// All token types recognized by the NileLang scanner.
/// </summary>
public enum TokenType
{
    // ── Keywords (NileLang → C# equivalent) ──
    Temple, Reign, Stone, Water, Papyrus, Maat,
    Judge, Banish, Flow, Dynasty, Carve, Listen, Tribute,

    // ── Control Flow ──
    Comma,           // ,
    Shatter,         // break
    Persist,         // continue

    // ── Literals & Identifiers ──
    Identifier,
    Integer,
    Float,
    StringLiteral,

    // ── Operators ──
    Assign,          // =
    Equals,          // ==
    Not,             // !
    NotEquals,       // !=
    GreaterThan,     // >
    GreaterOrEqual,  // >=
    LessThan,        // <
    LessOrEqual,     // <=
    Plus,            // +
    Increment,       // ++
    Minus,           // -
    Decrement,       // --
    Multiply,        // *
    Divide,          // /
    And,             // &&
    Or,              // ||
    True,
    False,

    // ── Delimiters ──
    LeftBrace,       // {
    RightBrace,      // }
    LeftParen,       // (
    RightParen,      // )
    Semicolon,       // ;

    // ── Special ──
    EOF,
    Unknown
}
