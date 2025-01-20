using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

public static class ConstantExpressionParser
{
    private static string Name => nameof(ConstantExpressionParser);

    public static ConstantExpression Parse(SqlTokenizer tokenizer)
    {
        var token = tokenizer.Read(Name, TokenType.Constant);
        return new ConstantExpression(token.Value);
    }
}
