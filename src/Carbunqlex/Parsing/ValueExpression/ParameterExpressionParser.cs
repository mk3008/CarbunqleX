using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpression;

public class ParameterExpressionParser
{
    public static ParameterExpression Parse(SqlTokenizer tokenizer)
    {
        var token = tokenizer.Read(TokenType.Parameter);
        return new ParameterExpression(token.Value);
    }
}
