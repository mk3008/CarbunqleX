using Carbunqlex.Clauses;
using Carbunqlex.Lexing;
using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing;

public class HavingClauseParser
{
    public static HavingClause Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read("having");
        var expression = ParseConditions(tokenizer);
        return new HavingClause(expression);
    }

    public static List<IValueExpression> ParseConditions(SqlTokenizer tokenizer)
    {
        return ValueExpressionListParser.Parse(tokenizer);
    }
}
