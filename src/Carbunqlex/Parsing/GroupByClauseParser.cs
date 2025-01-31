using Carbunqlex.Clauses;
using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing;

public class GroupByClauseParser
{
    private static string ParserName => nameof(GroupByClauseParser);

    public static GroupByClause Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read(ParserName, "group by");

        var expressions = new List<IValueExpression>();
        while (true)
        {
            var expression = ValueExpressionParser.Parse(tokenizer);
            expressions.Add(expression);
            if (tokenizer.IsEnd)
            {
                break;
            }
            var next = tokenizer.Peek();
            if (next.Type == TokenType.Comma)
            {
                tokenizer.Read();
                continue;
            }
            break;
        }

        return new GroupByClause(expressions);
    }
}
