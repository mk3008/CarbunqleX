using Carbunqlex.Clauses;
using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing;

public class GroupByClauseParser
{
    public static GroupByClause Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read("group by");
        var expressions = ParseGroupByColumns(tokenizer);
        return new GroupByClause(expressions);
    }

    public static List<IValueExpression> ParseGroupByColumns(SqlTokenizer tokenizer)
    {
        return ValueExpressionListParser.Parse(tokenizer);
    }
}

public static class ValueExpressionListParser
{
    public static List<IValueExpression> Parse(SqlTokenizer tokenizer)
    {
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
        return expressions;
    }
}
