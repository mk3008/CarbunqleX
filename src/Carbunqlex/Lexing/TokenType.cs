namespace Carbunqlex.Lexing;

public enum TokenType : byte
{
    Unknown,
    Literal,
    Operator,
    OpenParen,
    CloseParen,
    Comma,
    Dot,
    Identifier,
    Command,
    //Value,
    StartClause,
    EndClause,
    Parameter,
    OpenBracket,
    CloseBracket,
    Comment,
    EscapedStringConstant,
}
