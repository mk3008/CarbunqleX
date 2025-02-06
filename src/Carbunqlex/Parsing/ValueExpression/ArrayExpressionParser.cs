using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpression;

public static class ArrayExpressionParser
{
    public static ArrayExpression Parse(SqlTokenizer tokenizer)
    {
        var command = tokenizer.Read("array");

        var args = ValueArgumentsParser.Parse(tokenizer, TokenType.OpenBracket, TokenType.CloseBracket);

        return new ArrayExpression(args);
    }
}
