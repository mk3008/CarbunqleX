using Carbunqlex.Clauses;

namespace Carbunqlex.Parsing;

public class HavingClauseParser
{
    private static string ParserName => nameof(HavingClauseParser);

    public static HavingClause Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read(ParserName, "having");

        var expression = ValueExpressionParser.Parse(tokenizer);

        return new HavingClause(expression);
    }
}
