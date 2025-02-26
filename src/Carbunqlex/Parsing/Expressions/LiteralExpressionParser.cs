using Carbunqlex.Expressions;
using Carbunqlex.Lexing;

namespace Carbunqlex.Parsing.Expressions;

public static class LiteralExpressionParser
{
    public static LiteralExpression Parse(SqlTokenizer tokenizer)
    {
        var token = tokenizer.Read(TokenType.Literal);
        return new LiteralExpression(token.Value);
    }
}
