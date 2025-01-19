using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

public static class ValueExpressionParser
{
    private static string Name => nameof(ValueExpressionParser);

    public static IValueExpression Parse(SqlTokenizer tokenizer)
    {
        if (!tokenizer.TryPeek(out var token))
        {
            throw SqlParsingExceptionBuilder.EndOfInput(Name, tokenizer); ;
        }

        if (token.Type == TokenType.Identifier)
        {
            var expression = ColumnExpressionParser.Parse(tokenizer);

            if (!tokenizer.TryPeek(out token))
            {
                return expression;
            }
            if (token.Identifier == "between" || token.Identifier == "not between")
            {
                return BetweenExpressionParser.Parse(tokenizer, expression);
            }
        }

        throw SqlParsingExceptionBuilder.UnexpectedTokenType(Name, TokenType.Identifier, tokenizer, token);
    }
}
