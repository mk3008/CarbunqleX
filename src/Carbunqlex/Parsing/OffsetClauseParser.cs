using Carbunqlex.Clauses;
using Carbunqlex.Lexing;

namespace Carbunqlex.Parsing;

public static class OffsetClauseParser
{
    public static OffsetClause Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read("offset");
        var count = ValueExpressionParser.Parse(tokenizer);

        // row, rows は読み捨てます
        if (tokenizer.Peek(static t => t.CommandOrOperatorText is "row" or "rows" ? true : false, false))
        {
            tokenizer.CommitPeek();
        }

        return new OffsetClause(count);
    }
}
