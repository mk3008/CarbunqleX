using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

public static class FunctionExpressionParser
{
    private static string ParserName => nameof(FunctionExpressionParser);

    public static FunctionExpression Parse(SqlTokenizer tokenizer, Token function)
    {
        // TODO: Add support for DISTINCT, ALL keyword

        var args = ValueArgumentsParser.Parse(tokenizer, TokenType.OpenParen, TokenType.CloseParen);

        // TODO: Add support for OVER, FILTER, WITHIN GROUP clauses

        return new FunctionExpression(function.Value, string.Empty, args);
    }
}
