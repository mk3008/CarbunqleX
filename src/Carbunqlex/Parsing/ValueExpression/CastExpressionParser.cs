using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

public static class CastExpressionParser
{
    private static string ParserName => nameof(CastExpressionParser);

    public static CastExpression Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read(ParserName, "cast");

        tokenizer.Read(ParserName, TokenType.OpenParen);

        var expression = ValueExpressionParser.Parse(tokenizer);

        tokenizer.Read(ParserName, "as");

        var targetType = ValueExpressionParser.Parse(tokenizer);

        tokenizer.Read(ParserName, TokenType.CloseParen);

        return new CastExpression(expression, targetType.ToSqlWithoutCte());
    }
}
