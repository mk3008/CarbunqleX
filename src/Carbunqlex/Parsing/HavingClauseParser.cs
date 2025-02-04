using Carbunqlex.Clauses;

namespace Carbunqlex.Parsing;

public class HavingClauseParser
{
    public static HavingClause Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read("having");

        var expression = ValueExpressionParser.Parse(tokenizer);

        return new HavingClause(expression);
    }
}
