using Carbunqlex.Expressions;
using Carbunqlex.Lexing;

namespace Carbunqlex.Parsing.Expressions;

public class ParameterExpressionParser
{
    public static ParameterExpression Parse(SqlTokenizer tokenizer)
    {
        var token = tokenizer.Read(TokenType.Parameter);
        return new ParameterExpression(token.Value);
    }
}
