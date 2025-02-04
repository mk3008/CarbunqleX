using Carbunqlex.Clauses;

namespace Carbunqlex.Parsing;

public class WhereClauseParser
{
    public static WhereClause Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read("where");
        var condition = ValueExpressionParser.Parse(tokenizer);
        return new WhereClause(condition);
    }
}
