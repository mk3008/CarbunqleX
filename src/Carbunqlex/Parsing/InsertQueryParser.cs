using Carbunqlex.Clauses;
using Carbunqlex.DatasourceExpressions;
using Carbunqlex.Parsing.ValueExpression;

namespace Carbunqlex.Parsing;

/// <summary>
/// Parses INSERT queries from SQL tokens.
/// </summary>
public static class InsertQueryParser
{
    /// <summary>
    /// Parse INSERT query from SQL string.
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    public static InsertQuery Parse(string sql)
    {
        return Parse(new SqlTokenizer(sql));
    }

    /// <summary>
    /// Parse INSERT query from SQL tokenizer.
    /// </summary>
    /// <param name="tokenizer"></param>
    /// <returns></returns>
    public static InsertQuery Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read("insert into");

        // Parse InsertClause
        var insertClause = ParseInsertClause(tokenizer);

        // Parse SelectQuery
        var selectQuery = SelectQueryParser.ParseWithoutEndCheck(tokenizer);

        if (tokenizer.IsEnd)
        {
            var insertQuery = new InsertQuery(insertClause, selectQuery);
            return insertQuery;
        }
        else if (tokenizer.Peek().CommandOrOperatorText == "returning")
        {
            tokenizer.CommitPeek();
            var returning = ParseReturningClause(tokenizer);

            var insertQuery = new InsertQuery(insertClause, selectQuery, returning);
            if (tokenizer.IsEnd)
            {
                return insertQuery;
            }
            // Error if there are unparsed tokens left
            throw SqlParsingExceptionBuilder.Interrupted(tokenizer, insertQuery.ToSql());
        }
        else
        {
            var insertQuery = new InsertQuery(insertClause, selectQuery);
            // Error if there are unparsed tokens left
            throw SqlParsingExceptionBuilder.Interrupted(tokenizer, insertQuery.ToSql());
        }
    }

    /// <summary>
    /// Parse InsertClause
    /// </summary>
    /// <param name="tokenizer"></param>
    /// <returns></returns>
    private static InsertClause ParseInsertClause(SqlTokenizer tokenizer)
    {
        // Parse table name and column names
        var tableSource = ParseTableSource(tokenizer);

        // Read column names (optional)
        var columnNames = new List<string>();

        if (tokenizer.Peek().Type == TokenType.OpenParen)
        {
            tokenizer.CommitPeek();
            while (true)
            {
                var columnName = tokenizer.Read(TokenType.Identifier).Value;
                columnNames.Add(columnName);
                if (tokenizer.Peek().Type == TokenType.Comma)
                {
                    tokenizer.CommitPeek();
                    continue;
                }
                else if (tokenizer.Peek().Type == TokenType.CloseParen)
                {
                    break;
                }
                throw SqlParsingExceptionBuilder.UnexpectedTokenType(tokenizer, [TokenType.Comma, TokenType.CloseParen], tokenizer.Peek());
            }
            tokenizer.Read(TokenType.CloseParen);
        }

        // Create InsertClause
        return new InsertClause(tableSource, columnNames);
    }

    /// <summary>
    /// Parse table source
    /// </summary>
    /// <param name="tokenizer"></param>
    /// <returns></returns>
    private static TableSource ParseTableSource(SqlTokenizer tokenizer)
    {
        var items = new List<string>();

        while (true)
        {
            var identifier = tokenizer.Read(TokenType.Identifier).Value;
            items.Add(identifier);
            if (tokenizer.Peek().Type == TokenType.Dot)
            {
                tokenizer.CommitPeek();
                continue;
            }
            break;
        }

        if (items.Count == 0)
        {
            throw SqlParsingExceptionBuilder.UnexpectedTokenType(tokenizer, TokenType.Identifier, tokenizer.Peek());
        }
        else if (items.Count == 1)
        {
            return new TableSource(items[0]);
        }
        else
        {
            return new TableSource(items.Take(items.Count - 1).ToList(), items.Last());
        }
    }

    private static ReturningClause ParseReturningClause(SqlTokenizer tokenizer)
    {
        // Parse expressions
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
        return new ReturningClause(expressions);
    }
}
