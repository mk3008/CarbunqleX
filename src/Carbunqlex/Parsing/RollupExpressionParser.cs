using Carbunqlex.Parsing.ValueExpression;
using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing;

public static class RollupExpressionParser
{
    private static string ParserName => nameof(RollupExpressionParser);
    public static RollupExpression Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read(ParserName, "rollup");
        tokenizer.Read(ParserName, TokenType.OpenParen);
        var expressions = new List<IValueExpression>();
        while (true)
        {
            var expression = ValueExpressionParser.Parse(tokenizer);
            expressions.Add(expression);
            if (tokenizer.IsEnd)
            {
                break;
            }
            if (tokenizer.Peek().Type == TokenType.Comma)
            {
                tokenizer.Read();
                continue;
            }
            break;
        }
        tokenizer.Read(ParserName, TokenType.CloseParen);
        return new RollupExpression(expressions);
    }
}
