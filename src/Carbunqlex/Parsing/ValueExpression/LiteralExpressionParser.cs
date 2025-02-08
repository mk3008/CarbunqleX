using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpression;

public static class LiteralExpressionParser
{
    public static LiteralExpression Parse(SqlTokenizer tokenizer)
    {
        var token = tokenizer.Read(TokenType.Literal);
        return new LiteralExpression(token.Value);
    }
}
