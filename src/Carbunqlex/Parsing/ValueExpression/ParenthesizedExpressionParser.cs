using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpression;

public class ParenthesizedExpressionParser
{
    public static IValueExpression Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read(TokenType.OpenParen);

        if (tokenizer.Peek().CommandOrOperatorText is "select" or "with" or "values")
        {
            var query = SelectQueryParser.ParseWithoutEndCheck(tokenizer);
            tokenizer.Read(TokenType.CloseParen);
            return new InlineQuery(query);
        }
        else
        {
            var values = new List<IValueExpression>() {
                ValueExpressionParser.Parse(tokenizer)
            };
            while (tokenizer.TryPeek(out var comma) && comma.Type == TokenType.Comma)
            {
                tokenizer.CommitPeek();
                values.Add(ValueExpressionParser.Parse(tokenizer));
            }
            tokenizer.Read(TokenType.CloseParen);
            if (values.Count == 1)
            {
                return new ParenthesizedExpression(values[0]);
            }
            else
            {
                return new InValueGroupExpression(values);
            }
        }
    }
}
