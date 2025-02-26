using Carbunqlex.Expressions;
using Carbunqlex.Lexing;

namespace Carbunqlex.Parsing.Expressions;

public class UnaryExpressionParser
{
    public static UnaryExpression Parse(SqlTokenizer tokenizer, string @operator)
    {
        return new UnaryExpression(@operator, ValueExpressionParser.Parse(tokenizer));
    }
}
