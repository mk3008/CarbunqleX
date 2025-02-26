using Carbunqlex.Clauses;
using Carbunqlex.Lexing;

namespace Carbunqlex.Parsing.Clauses;

public class FilterClauseParser
{
    public static FilterClause Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read("filter");
        tokenizer.Read(TokenType.OpenParen);

        var whereClause = WhereClauseParser.Parse(tokenizer);
        tokenizer.Read(TokenType.CloseParen);

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
