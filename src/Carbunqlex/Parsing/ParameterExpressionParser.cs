using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing;

public class ParameterExpressionParser
{
    private static string ParserName => nameof(ParameterExpressionParser);

    public static ParameterExpression Parse(SqlTokenizer tokenizer)
    {
        var token = tokenizer.Read(ParserName, TokenType.Parameter);
        return new ParameterExpression(token.Value);
    }
}
