# NileLang — Context-Free Grammar (CFG)
# Notation: EBNF
# Derived from Parser.cs implementation
#
# Legend:
#   <Name>     = Non-terminal
#   KEYWORD    = Terminal token (exact keyword)
#   { ... }    = Repetition (zero or more)
#   [ ... ]    = Optional (zero or one)
#   |          = Alternation (choose one)
#   ( ... )    = Grouping
#   ε          = Epsilon (empty / nothing)

# =============================================================================
# 1. TOP-LEVEL
# =============================================================================

<Program>       →  { <Statement> }

# =============================================================================
# 2. STATEMENTS
# =============================================================================

<Statement>     →  <VarDecl>
                |  <Assignment>
                |  <PrintStmt>
                |  <IfStmt>
                |  <WhileStmt>
                |  <Block>
                |  <FunctionDecl>
                |  <ReturnStmt>
                |  <BreakStmt>
                |  <ContinueStmt>

# =============================================================================
# 3. VARIABLE DECLARATION & ASSIGNMENT
# =============================================================================

<VarDecl>       →  <Type> IDENTIFIER = <Expr> ;
<Assignment>    →  IDENTIFIER = <Expr> ;
<Type>          →  stone | water | papyrus | maat

# =============================================================================
# 4. PRINT
# =============================================================================

<PrintStmt>     →  carve ( <Expr> ) ;

# =============================================================================
# 5. CONTROL FLOW
# =============================================================================

<IfStmt>        →  judge ( <Expr> ) <Block> [ banish <Block> ]
<WhileStmt>     →  flow ( <Expr> ) <Block>
<BreakStmt>     →  shatter ;
<ContinueStmt>  →  persist ;
<Block>         →  { { <Statement> } }

# =============================================================================
# 6. FUNCTIONS
# =============================================================================

<FunctionDecl>  →  dynasty IDENTIFIER ( [ <ParamList> ] ) <Block>
<ParamList>     →  <Param> { , <Param> }
<Param>         →  <Type> IDENTIFIER
<ReturnStmt>    →  tribute [ <Expr> ] ;

# =============================================================================
# 7. EXPRESSIONS  (ordered low → high precedence)
# =============================================================================

<Expr>          →  <LogicOr>

<LogicOr>       →  <LogicAnd>    { || <LogicAnd>    }
<LogicAnd>      →  <Equality>    { && <Equality>    }
<Equality>      →  <Comparison>  { ( == | != ) <Comparison>  }
<Comparison>    →  <Term>        { ( > | >= | < | <= ) <Term> }
<Term>          →  <Factor>      { ( + | - ) <Factor> }
<Factor>        →  <Unary>       { ( * | / ) <Unary>  }

<Unary>         →  ( ! | - ) <Unary>
                |  <Call>

<Call>          →  <Primary> [ ( [ <ArgList> ] ) ]
<ArgList>       →  <Expr> { , <Expr> }

<Primary>       →  INTEGER
                |  FLOAT
                |  STRING
                |  true
                |  false
                |  IDENTIFIER
                |  ( <Expr> )

# =============================================================================
# 8. TERMINALS  (produced by Scanner.cs)
# =============================================================================

INTEGER         →  [0-9]+
FLOAT           →  [0-9]+ . [0-9]+
STRING          →  " { any char except " } "
IDENTIFIER      →  [A-Za-z] { [A-Za-z0-9_] }


