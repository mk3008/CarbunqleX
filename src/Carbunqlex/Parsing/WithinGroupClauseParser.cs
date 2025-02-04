using Carbunqlex.Clauses;

namespace Carbunqlex.Parsing;

public class WithinGroupClauseParser
{
    public static WithinGroupClause Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read("within group");
        tokenizer.Read(TokenType.OpenParen);
        var orderBy = OrderByClauseParser.Parse(tokenizer);
        tokenizer.Read(TokenType.CloseParen);

        return new WithinGroupClause(orderBy);
    }
}
