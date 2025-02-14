using Carbunqlex.Clauses;
using Carbunqlex.Parsing.ValueExpression;
using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing;

/// <summary>
/// Parses a SELECT query.
/// </summary>
public static class SelectQueryParser
{
    /// <summary>
    /// Parse a SELECT query.
    /// Throws an error if the end of the query is not reached.
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    public static ISelectQuery Parse(string sql)
    {
        return Parse(new SqlTokenizer(sql));
    }

    /// <summary>
    /// Parse a SELECT query.
    /// Throws an error if the end of the query is not reached.
    /// </summary>
    /// <param name="tokenizer"></param>
    /// <returns></returns>
    public static ISelectQuery Parse(SqlTokenizer tokenizer)
    {
        var selectQuery = ParseWithoutEndCheck(tokenizer);
        if (!tokenizer.IsEnd)
        {
            throw SqlParsingExceptionBuilder.Interrupted(tokenizer, selectQuery.ToSql());
        }
        return selectQuery;
    }

    /// <summary>
    /// Parse a SELECT query without checking the end of the query.
    /// </summary>
    /// <param name="tokenizer"></param>
    /// <returns></returns>
    internal static ISelectQuery ParseWithoutEndCheck(SqlTokenizer tokenizer)
    {
        var selectQuery = ParseCore(tokenizer);

        var isUnion = tokenizer.Peek(static t =>
        {
            return SqlKeyword.UnionCommandKeywords.Contains(t.CommandOrOperatorText)
                ? true
                : false;
        }, false);

        if (isUnion)
        {
            return ParseUnion(tokenizer, selectQuery);
        }
        return selectQuery;
    }

    private static ISelectQuery ParseCore(SqlTokenizer tokenizer)
    {
        var next = tokenizer.Peek();

        if (next.CommandOrOperatorText == "with")
        {
            tokenizer.CommitPeek();
            var commonTables = WithClauseParser.ParseCommonTables(tokenizer);
            var query = ParseWithoutCte(tokenizer);
            query.WithClause.AddRange(commonTables);
            return query;
        }
        else if (next.CommandOrOperatorText == "select")
        {
            return ParseWithoutCte(tokenizer);
        }
        else if (next.CommandOrOperatorText == "values")
        {
            return ParseValuesQuery(tokenizer);
        }

        throw SqlParsingExceptionBuilder.UnexpectedToken(tokenizer, ["select", "with", "values"], next);
    }

    private static ISelectQuery ParseValuesQuery(SqlTokenizer tokenizer)
    {
        tokenizer.Read("values");
        var rows = ParseValuesRows(tokenizer).ToList();
        return new ValuesQuery(rows);
    }

    private static IEnumerable<ValuesRow> ParseValuesRows(SqlTokenizer tokenizer)
    {
        while (true)
        {
            yield return ParseValuesRow(tokenizer);
            if (tokenizer.IsEnd)
            {
                break;
            }
            if (tokenizer.Peek().Type == TokenType.Comma)
            {
                tokenizer.CommitPeek();
                continue;
            }
            break;
        }
    }

    private static ValuesRow ParseValuesRow(SqlTokenizer tokenizer)
    {
        tokenizer.Read(TokenType.OpenParen);
        var columns = new List<IValueExpression>();
        while (true)
        {
            columns.Add(ValueExpressionParser.Parse(tokenizer));
            if (tokenizer.Peek().Type == TokenType.Comma)
            {
                tokenizer.CommitPeek();
                continue;
            }
            break;
        }
        tokenizer.Read(TokenType.CloseParen);
        return new ValuesRow(columns);
    }

    public static SelectQuery ParseWithoutCte(SqlTokenizer tokenizer)
    {
        tokenizer.Read("select");

        var selectClause = ParseSelectClause(tokenizer);

        if (tokenizer.IsEnd || tokenizer.Peek().CommandOrOperatorText != "from")
        {
            return new SelectQuery(selectClause);
        }

        var fromClause = FromClauseParser.Parse(tokenizer);

        var selectQuery = new SelectQuery(selectClause, fromClause);

        if (!tokenizer.IsEnd && tokenizer.Peek().CommandOrOperatorText == "where")
        {
            tokenizer.CommitPeek();
            selectQuery.WhereClause.Add(WhereClauseParser.ParseCondition(tokenizer));
        }

        if (!tokenizer.IsEnd && tokenizer.Peek().CommandOrOperatorText == "group by")
        {
            tokenizer.CommitPeek();
            selectQuery.GroupByClause.AddRange(GroupByClauseParser.ParseGroupByColumns(tokenizer));
        }

        if (!tokenizer.IsEnd && tokenizer.Peek().CommandOrOperatorText == "having")
        {
            tokenizer.CommitPeek();
            selectQuery.HavingClause.AddRange(HavingClauseParser.ParseConditions(tokenizer));
        }

        if (!tokenizer.IsEnd && tokenizer.Peek().CommandOrOperatorText == "order by")
        {
            tokenizer.CommitPeek();
            selectQuery.OrderByClause.AddRange(OrderByClauseParser.ParseOrderByColumns(tokenizer));
        }

        if (!tokenizer.IsEnd && tokenizer.Peek().CommandOrOperatorText == "window")
        {
            tokenizer.CommitPeek();
            selectQuery.WindowClause.AddRange(WindowClauseParser.ParseWindowExpressions(tokenizer));
        }

        if (!tokenizer.IsEnd && tokenizer.Peek().CommandOrOperatorText == "limit")
        {
            selectQuery.LimitClause = LimitClauseParser.Parse(tokenizer);
        }

        if (!tokenizer.IsEnd && tokenizer.Peek().CommandOrOperatorText == "offset")
        {
            selectQuery.OffsetClause = OffsetClauseParser.Parse(tokenizer);
        }

        if (!tokenizer.IsEnd && tokenizer.Peek().CommandOrOperatorText == "fetch")
        {
            // fetch も limit と同じクラスを使う
            selectQuery.LimitClause = LimitClauseParser.Parse(tokenizer);
        }

        if (!tokenizer.IsEnd && tokenizer.Peek().CommandOrOperatorText == "for")
        {
            selectQuery.ForClause = ForClauseParser.Parse(tokenizer);
        }

        return selectQuery;
    }

    private static SelectClause ParseSelectClause(SqlTokenizer tokenizer)
    {
        // distinctClause
        IDistinctClause? distinctClause = tokenizer.Peek(static (r, t) =>
        {
            return t.CommandOrOperatorText switch
            {
                "distinct" => (IDistinctClause)DistinctClauseParser.Parse(r),
                "distinct on" => DistinctOnClauseParser.Parse(r),
                _ => null
            };
        });

        // expressions
        var expressions = new List<SelectExpression>();
        while (true)
        {
            expressions.Add(SelectExpressionParser.Parse(tokenizer));
            if (tokenizer.IsEnd)
            {
                break;
            }
            if (tokenizer.Peek().Type == TokenType.Comma)
            {
                tokenizer.CommitPeek();
                continue;
            }
            break;
        }
        return new SelectClause(distinctClause, expressions);
    }

    private static class DistinctClauseParser
    {
        public static DistinctClause Parse(SqlTokenizer tokenizer)
        {
            tokenizer.Read("distinct");
            return new DistinctClause();
        }
    }

    private static class DistinctOnClauseParser
    {
        public static DistinctOnClause Parse(SqlTokenizer tokenizer)
        {
            tokenizer.Read("distinct on");
            tokenizer.Read(TokenType.OpenParen);

            var expressions = new List<IValueExpression>();
            while (true)
            {
                var expression = ValueExpressionParser.Parse(tokenizer);
                expressions.Add(expression);
                if (tokenizer.IsEnd)
                {
                    break;
                }
                if (tokenizer.Peek().Type == TokenType.Comma)
                {
                    tokenizer.Read();
                    continue;
                }
                break;
            }
            tokenizer.Read(TokenType.CloseParen);
            return new DistinctOnClause(expressions);
        }
    }

    /// <summary>
    /// Parses a UNION query.
    /// Removes parentheses if enclosed.
    /// </summary>
    /// <param name="tokenizer"></param>
    /// <param name="left"></param>
    /// <returns></returns>
    internal static UnionQuery ParseUnion(SqlTokenizer tokenizer, ISelectQuery left)
    {
        var unionType = tokenizer.Read(SqlKeyword.UnionCommandKeywords).Value;

        if (tokenizer.Peek().CommandOrOperatorText is "select" or "with" or "values")
        {
            var right = ParseWithoutEndCheck(tokenizer);
            return new UnionQuery(unionType, left, right);
        }
        else if (tokenizer.Peek().Type == TokenType.OpenParen)
        {
            var right = ParseSubQuery(tokenizer);
            return new UnionQuery(unionType, left, right);
        }
        throw SqlParsingExceptionBuilder.UnexpectedToken(tokenizer, ["select", "with", "values"], tokenizer.Peek());
    }

    /// <summary>
    /// Parses a subquery enclosed in parentheses. 
    /// Removes excessive parentheses if used.
    /// </summary>
    /// <param name="tokenizer"></param>
    /// <returns></returns>
    internal static ISelectQuery ParseSubQuery(SqlTokenizer tokenizer)
    {
        tokenizer.Read(TokenType.OpenParen);

        if (tokenizer.Peek().CommandOrOperatorText is "select" or "with" or "values")
        {
            var query = ParseWithoutEndCheck(tokenizer);
            tokenizer.Read(TokenType.CloseParen);
            return query;
        }
        else if (tokenizer.Peek().Type == TokenType.OpenParen)
        {
            var query = ParseSubQuery(tokenizer);
            if (SqlKeyword.UnionCommandKeywords.Contains(tokenizer.Peek().CommandOrOperatorText))
            {
                query = ParseUnion(tokenizer, query);
            }
            tokenizer.Read(TokenType.CloseParen);
            return query;
        }
        throw SqlParsingExceptionBuilder.UnexpectedToken(tokenizer, ["select", "with", "values"], tokenizer.Peek());
    }
}
