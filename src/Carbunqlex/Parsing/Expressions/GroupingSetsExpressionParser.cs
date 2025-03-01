using Carbunqlex.Expressions;
using Carbunqlex.Lexing;

namespace Carbunqlex.Parsing.Expressions;

public static class GroupingSetsExpressionParser
{
    public static GroupingSetsExpression Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read("grouping sets");
        tokenizer.Read(TokenType.OpenParen);
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
        tokenizer.Read(TokenType.CloseParen);
        return new GroupingSetsExpression(expressions);
    }
}
