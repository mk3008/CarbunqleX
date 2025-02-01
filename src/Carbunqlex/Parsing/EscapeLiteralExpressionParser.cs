using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing;

public static class EscapeLiteralExpressionParser
{
    private static string ParserName => nameof(EscapeLiteralExpressionParser);
    public static EscapeLiteralExpression Parse(SqlTokenizer tokenizer)
    {
        var token = tokenizer.Read(ParserName, TokenType.EscapedStringConstant);

        if (tokenizer.IsEnd)
        {
            return new EscapeLiteralExpression(token.Value);
        }

        if (tokenizer.Peek().Identifier == "uescape")
        {
            tokenizer.CommitPeek();
            var escapeOption = tokenizer.Read(ParserName, TokenType.Constant).Value;
            return new EscapeLiteralExpression(token.Value, escapeOption);
        }

        return new EscapeLiteralExpression(token.Value);
    }
}
