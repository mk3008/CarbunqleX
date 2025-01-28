using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpression;

public class UnaryExpressionParser
{
    private static string ParserName => nameof(UnaryExpressionParser);

    public static UnaryExpression Parse(SqlTokenizer tokenizer, string @operator)
    {
        return new UnaryExpression(@operator, ValueExpressionParser.Parse(tokenizer));
    }
}
