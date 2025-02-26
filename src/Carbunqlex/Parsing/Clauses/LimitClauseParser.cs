using Carbunqlex.Clauses;
using Carbunqlex.Lexing;
using Carbunqlex.Parsing.Expressions;

namespace Carbunqlex.Parsing.Clauses;

public class LimitClauseParser
{
    public static ILimitClause Parse(SqlTokenizer tokenizer)
    {
        var next = tokenizer.Peek();

        if (next.CommandOrOperatorText == "limit")
        {
            return ParseLimit(tokenizer);
        }
        else if (next.CommandOrOperatorText == "fetch")
        {
            return ParseFetch(tokenizer);
        }
        throw SqlParsingExceptionBuilder.UnexpectedToken(tokenizer, ["limit", "fetch"], next);
    }

    private static LimitClause ParseLimit(SqlTokenizer tokenizer)
    {
        tokenizer.Read("limit");

        var count = ValueExpressionParser.Parse(tokenizer);

        if (!tokenizer.IsEnd)
        {
            // ignore keywords
            tokenizer.Peek(static (r, t) =>
            {
                if (t.CommandOrOperatorText is "rows" or "only")
                {
                    r.CommitPeek();
                    return t.Value;
                }
                return string.Empty;
            });
        }

        return new LimitClause(count);
    }

    public static FetchClause ParseFetch(SqlTokenizer tokenizer)
    {
        tokenizer.Read("fetch");

        var fetchType = tokenizer.Read(TokenType.Command);
        if (fetchType.CommandOrOperatorText != "first" && fetchType.CommandOrOperatorText != "next")
        {
            throw SqlParsingExceptionBuilder.UnexpectedToken(tokenizer, ["first", "next"], fetchType);
        }

        var count = ValueExpressionParser.Parse(tokenizer);

        var isPercentage = tokenizer.Peek(static (r, t) =>
        {
            if (t.CommandOrOperatorText is "percent")
            {
                r.CommitPeek();
                return true;
            }
            return false;
        }, false);

        if (!tokenizer.IsEnd)
        {
            // ignore keywords
            tokenizer.Peek(static (r, t) =>
            {
                if (t.CommandOrOperatorText is "row" or "rows" or "rows only")
                {
                    r.CommitPeek();
                    return t.Value;
                }
                return string.Empty;
            });
        }

        var withTies = tokenizer.Peek(static (r, t) =>
        {
            if (t.CommandOrOperatorText is "with ties")
            {
                r.CommitPeek();
                return t.Value;
            }
            return string.Empty;
        }, string.Empty);

        return new FetchClause(fetchType.Value, count, isPercentage, withTies);
    }
}
