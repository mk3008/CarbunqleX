using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

public static class BinaryExpressionParser
{
    private static string ParserName => nameof(BinaryExpressionParser);

    public static BinaryExpression Parse(SqlTokenizer tokenizer, IValueExpression left)
    {
        var operatorToken = tokenizer.Read(ParserName, TokenType.Operator);
        var right = ValueExpressionParser.Parse(tokenizer);
        return new BinaryExpression(operatorToken.Value, left, right);
    }
}
