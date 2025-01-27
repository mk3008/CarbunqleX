using Carbunqlex.Clauses;

namespace Carbunqlex.Parsing;

public class WhereClauseParser
{
    private static string ParserName => nameof(WhereClauseParser);

    public static WhereClause Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read(ParserName, "where");
        var condition = ValueExpressionParser.Parse(tokenizer);
        return new WhereClause(condition);
    }
}
