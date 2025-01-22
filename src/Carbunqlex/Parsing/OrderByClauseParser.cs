using Carbunqlex.Clauses;
using Carbunqlex.Parsing.ValueExpressionParsing;

namespace Carbunqlex.Parsing;

public class OrderByClauseParser
{
    private static string ParserName => nameof(OrderByClauseParser);

    public static OrderByClause Parse(SqlTokenizer tokenizer)
    {
        if (!tokenizer.TryPeek(out var token))
        {
            throw SqlParsingExceptionBuilder.EndOfInput(ParserName, tokenizer); ;
        }

        tokenizer.Read(ParserName, "order by");

        var expressions = new List<OrderByColumn>();
        do
        {
            var expression = OrderByColumnParser.Parse(tokenizer);
            expressions.Add(expression);
        } while (tokenizer.TryPeek(out var comma) && comma.Type == TokenType.Comma);

        return new OrderByClause(expressions);
    }

    private class OrderByColumnParser
    {
        private static string ParserName => nameof(OrderByColumnParser);

        public static OrderByColumn Parse(SqlTokenizer tokenizer)
        {
            if (!tokenizer.TryPeek(out var token))
            {
                throw SqlParsingExceptionBuilder.EndOfInput(ParserName, tokenizer); ;
            }

            var expression = ValueExpressionParser.Parse(tokenizer);

            if (tokenizer.TryPeek(out var nextToken) && (nextToken.Identifier == "asc" || nextToken.Identifier == "desc"))
            {
                tokenizer.Read();
                return new OrderByColumn(expression, nextToken.Identifier == "asc");
            }

            return new OrderByColumn(expression);
        }
    }
}
