using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing;

public class UnaryExpressionParser
{
    private static string ParserName => nameof(UnaryExpressionParser);

    public static UnaryExpression Parse(SqlTokenizer tokenizer, string @operator)
    {
        return new UnaryExpression(@operator, ValueExpressionParser.Parse(tokenizer));
    }
}
