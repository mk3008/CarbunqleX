using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpression;

public class ParenthesizedExpressionParser
{
    public static IValueExpression Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read(TokenType.OpenParen);

        if (tokenizer.Peek().CommandOrOperatorText == "select")
        {
            var query = SelectQueryParser.Parse(tokenizer);
            tokenizer.Read(TokenType.CloseParen);
            return new InlineQuery(query);
        }
        else
        {
            var value = ValueExpressionParser.Parse(tokenizer);
            tokenizer.Read(TokenType.CloseParen);
            return new ParenthesizedExpression(value);
        }
    }
}
