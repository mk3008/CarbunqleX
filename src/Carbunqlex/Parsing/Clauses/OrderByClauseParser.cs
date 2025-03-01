using Carbunqlex.Clauses;
using Carbunqlex.Lexing;

namespace Carbunqlex.Parsing.Clauses;

public class OrderByClauseParser
{
    public static OrderByClause Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read("order by");
        var expressions = ParseOrderByColumns(tokenizer);
        return new OrderByClause(expressions);
    }

    public static List<OrderByColumn> ParseOrderByColumns(SqlTokenizer tokenizer)
    {
        var expressions = new List<OrderByColumn>();
        while (true)
        {
            var expression = OrderByColumnParser.Parse(tokenizer);
            expressions.Add(expression);
            if (!tokenizer.TryPeek(out var comma) || comma.Type != TokenType.Comma)
            {
                break;
            }
            tokenizer.Read();
        }
        return expressions;
    }
}
