using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing;

public class ParenthesizedExpressionParser
{
    public static ParenthesizedExpression Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read(TokenType.OpenParen);
        var value = ValueExpressionParser.Parse(tokenizer);
        tokenizer.Read(TokenType.CloseParen);

        return new ParenthesizedExpression(value);
    }
}
