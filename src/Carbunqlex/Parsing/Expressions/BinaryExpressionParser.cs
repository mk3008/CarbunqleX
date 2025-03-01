using Carbunqlex.Expressions;
using Carbunqlex.Lexing;

namespace Carbunqlex.Parsing.Expressions;

public static class BinaryExpressionParser
{
    public static BinaryExpression Parse(SqlTokenizer tokenizer, IValueExpression left)
    {
        var operatorToken = tokenizer.Read(TokenType.Operator);
        var right = ValueExpressionParser.Parse(tokenizer);
        return new BinaryExpression(operatorToken.Value, left, right);
    }
}
