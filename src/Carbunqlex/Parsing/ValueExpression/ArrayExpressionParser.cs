using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

public static class ArrayExpressionParser
{
    public static ArrayExpression Parse(SqlTokenizer tokenizer)
    {
        var command = tokenizer.Read("array");

        var args = ValueArgumentsParser.Parse(tokenizer, TokenType.OpenBracket, TokenType.CloseBracket);

        return new ArrayExpression(args);
    }
}
