namespace Carbunqlex;

public enum TokenType : byte
{
    Unknown,
    Constant,
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
