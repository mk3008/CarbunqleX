using Carbunqlex.Parsing.ValueExpression;

namespace Carbunqlex.Parsing;

public static class GroupingSetsExpressionParser
{
    private static string ParserName => nameof(GroupingSetsExpressionParser);
    public static GroupingSetsExpression Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read(ParserName, "grouping sets");
        tokenizer.Read(ParserName, TokenType.OpenParen);
        var expressions = new List<GroupingSetExpression>();
        while (true)
        {
            var expression = GroupingSetParser.Parse(tokenizer);
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
        return new GroupingSetsExpression(expressions);
    }
}
