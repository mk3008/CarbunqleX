using Carbunqlex.Clauses;

namespace Carbunqlex.Parsing;

public class WithinGroupClauseParser
{
    private static string ParserName => nameof(FilterClauseParser);

    public static WithinGroupClause Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read(ParserName, "within group");
        tokenizer.Read(ParserName, TokenType.OpenParen);
        var orderBy = OrderByClauseParser.Parse(tokenizer);
        tokenizer.Read(ParserName, TokenType.CloseParen);

        return new WithinGroupClause(orderBy);
    }
}
