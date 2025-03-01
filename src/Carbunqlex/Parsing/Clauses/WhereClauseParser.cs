using Carbunqlex.Clauses;
using Carbunqlex.Expressions;
using Carbunqlex.Lexing;
using Carbunqlex.Parsing.Expressions;

namespace Carbunqlex.Parsing.Clauses;

public class WhereClauseParser
{
    public static WhereClause Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read("where");
        var condition = ParseCondition(tokenizer);
        return new WhereClause(condition);
    }

    public static IValueExpression ParseCondition(SqlTokenizer tokenizer)
    {
        return ValueExpressionParser.Parse(tokenizer);
    }
}
