using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing;

public class ParenthesizedExpressionParser
{
    private static string ParserName => nameof(ParenthesizedExpressionParser);

    public static ParenthesizedExpression Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read(ParserName, TokenType.OpenParen);
        var value = ValueExpressionParser.Parse(tokenizer);
        tokenizer.Read(ParserName, TokenType.CloseParen);

        return new ParenthesizedExpression(value);
    }
}
