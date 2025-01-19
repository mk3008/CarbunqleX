using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

public static class ConstantExpressionParser
{
    private static string Name => nameof(ConstantExpressionParser);

    public static ConstantExpression Parse(SqlTokenizer tokenizer)
    {
        var defaultPosition = tokenizer.Position;
        if (!tokenizer.TryRead(out var token))
        {
            throw SqlParsingExceptionBuilder.EndOfInput(Name, tokenizer);
        }
        if (token.Type == TokenType.Constant)
        {
            return new ConstantExpression(token.Value);
        }

        throw SqlParsingExceptionBuilder.UnexpectedTokenType(Name, TokenType.Constant, tokenizer, token);
    }
}
