using Carbunqlex.Clauses;

namespace Carbunqlex.Parsing;

public class FilterClauseParser
{
    private static string ParserName => nameof(FilterClauseParser);

    public static FilterClause Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read(ParserName, "filter");
        tokenizer.Read(ParserName, TokenType.OpenParen);

        var whereClause = WhereClauseParser.Parse(tokenizer);
        tokenizer.Read(ParserName, TokenType.CloseParen);

        if (tokenizer.IsEnd)
        {
            return new FilterClause(whereClause);
        }

        var next = tokenizer.Peek();
        if (next.CommandOrOperatorText == "over")
        {
            var overClause = OverClauseParser.Parse(tokenizer);
            return new FilterClause(whereClause, overClause);
        }

        return new FilterClause(whereClause);
    }
}
