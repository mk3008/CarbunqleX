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
    Keyword,
    Value,
    StartClause,
    EndClause,
    Parameter,
    OpenBracket,
    CloseBracket,
    Comment,
}
