using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace NileLangCompiler;


public class Scanner
{
    private static readonly string _pattern =
        @"(?<Comment>(//.*?$)|(/\*[\s\S]*?\*/))|" +
        @"(?<Keyword>\b(temple|reign|stone|papyrus|maat|judge|banish|flow|dynasty|carve|listen|tribute)\b)|" +
        @"(?<Number>\d+)|" +
        @"(?<Identifier>[A-Za-z]\w*)|" +
        @"(?<Operator>(==|!=|>=|<=|\+\+|--|[=><+\-!]))|" +
        @"(?<Symbol>[{};()])|" +
        @"(?<StringLiteral>"".*?"")|" +
        @"(?<Whitespace>\s+)|"+
        @"(?<Unknown>.)";

     
    private static readonly Regex _regex = new Regex(_pattern, RegexOptions.Compiled|RegexOptions.Multiline);


    public List<Token> Scan(string sourceCode)
    {
        var tokens = new List<Token>();

        // iterator
    
        MatchCollection matches = _regex.Matches(sourceCode);

        foreach (Match match in matches)
        {
            // ignore white spaces 

            if (match.Groups["Whitespace"].Success || match.Groups["Comment"].Success)
            {
                continue;
            }
            
            // for unkown types 

            TokenType type = TokenType.Unknown;
            string lexeme = match.Value;

            // keywords 

            if (match.Groups["Keyword"].Success)
            {
                // Converts the string "stone" to the TokenType.Stone enum automatically
                Enum.TryParse(lexeme, true, out type); 
            }

            // numbers 

            else if (match.Groups["Number"].Success) type = TokenType.Number;

            // Identifiers

            else if (match.Groups["Identifier"].Success) type = TokenType.Identifier;

            // string literal 

            else if (match.Groups["StringLiteral"].Success) type = TokenType.StringLiteral;

            // operators

          else if (match.Groups["Operator"].Success)
            {
                type = lexeme switch
                {
                    "==" => TokenType.Equals,
                    "!=" => TokenType.NotEquals,
                    ">=" => TokenType.GreaterOrEqual,
                    "<=" => TokenType.LessOrEqual,
                    "++" => TokenType.Increment,
                    "--" => TokenType.Decrement,
                    "=" => TokenType.Assign,
                    ">" => TokenType.GreaterThan,
                    "<" => TokenType.LessThan,
                    "+" => TokenType.Plus,
                    "-" => TokenType.Minus,
                    "!" => TokenType.Not,
                    _ => TokenType.Unknown
                };
            }
            else if (match.Groups["Symbol"].Success)
            {
                type = lexeme switch
                {
                    "{" => TokenType.LeftBrace,
                    "}" => TokenType.RightBrace,
                    "(" => TokenType.LeftParen,
                    ")" => TokenType.RightParen,
                    ";" => TokenType.Semicolon,
                    _ => TokenType.Unknown
                };
            }else if (match.Groups["Unknown"].Success)
            {
                Console.WriteLine($"[WARNING] Unrecognized symbol '{lexeme}' found! The scribe cannot translate this.");
                type = TokenType.Unknown;
            }

            // Create the token and add it to our list
            tokens.Add(new Token(type, lexeme,1));
        }

        // Add an End-Of-File token so the Parser knows when to stop later
        tokens.Add(new Token(TokenType.EOF, "",1));
        return tokens;
    }


    

    
}

