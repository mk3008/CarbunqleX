namespace Carbunqlex;

public enum LexType : byte
{
    Unknown,
    Constant,
    Operator,
    OpenParen,
    CloseParen,
    ArgumentSplitterComma,
    Dot,
    Identifier,
    Keyword,
}
