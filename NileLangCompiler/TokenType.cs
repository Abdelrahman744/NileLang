namespace NileLangCompiler;

public enum TokenType
{
    // Reserved Words
    /*
     templet - class
     reign - main
     stone - int
     water - float
     papyrus - string 
     maat - boolean 
     judge - if
     banish - else 
     flow - while loop 
     dynasty -for loop 
     carve - console.writeline
     listen - console,readline

     tribute - return 
    
    
    */
    Temple, Reign, Stone, Water ,Papyrus, Maat, Judge, Banish, Flow, Dynasty, Carve, Listen, Tribute,



    // Other Tokens
    Identifier, 
    Integer,     
    Float,
    StringLiteral,

    //  Symbols & OperatorsAssign,         
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
    And,             // &&  
    Or,              // ||   


    LeftBrace,       // {
    RightBrace,      // }
    LeftParen,       // (
    RightParen,      // )
    Semicolon,       // ;

    // End of File 
    EOF,
    
    // For invalid characters
    Unknown

}