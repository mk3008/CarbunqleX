using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

public static class ConstantExpressionParser
{
    public static ConstantExpression Parse(SqlTokenizer tokenizer)
    {
        var token = tokenizer.Read(TokenType.Constant);
        return new ConstantExpression(token.Value);
    }
}
