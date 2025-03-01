using Carbunqlex.Clauses;
using Carbunqlex.Lexing;
using Carbunqlex.Parsing.Expressions;

namespace Carbunqlex.Parsing.Clauses;

public class OrderByColumnParser
{
    public static OrderByColumn Parse(SqlTokenizer tokenizer)
    {
        var expression = ValueExpressionParser.Parse(tokenizer);

        bool ascending = ParseAscending(tokenizer);
        bool? nullsFirst = ParseNullsFirst(tokenizer);

        return new OrderByColumn(expression, ascending, nullsFirst);
    }

    private static bool ParseAscending(SqlTokenizer tokenizer)
    {
        if (tokenizer.IsEnd)
        {
            return true;
        }

        var next = tokenizer.Peek().CommandOrOperatorText;

        if (next == "asc" || next == "desc")
        {
            tokenizer.CommitPeek();
            return next == "asc";
        }

        return true;
    }

    private static bool? ParseNullsFirst(SqlTokenizer tokenizer)
    {
        if (tokenizer.IsEnd)
        {
            return null;
        }

        var next = tokenizer.Peek().CommandOrOperatorText;

        if (next == "nulls first" || next == "nulls last")
        {
            tokenizer.Read();
            return next == "nulls first";
        }

        return null;
    }
}
