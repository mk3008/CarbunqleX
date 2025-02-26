using Carbunqlex.Clauses;
using Carbunqlex.Lexing;
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
            var returning = ReturningClauseParser.Parse(tokenizer);

            var insertQuery = new InsertQuery(insertClause, selectQuery, returning);
            if (tokenizer.IsEnd)
            {
                return insertQuery;
            }
            // Error if there are unparsed tokens left
            throw SqlParsingExceptionBuilder.Interrupted(tokenizer);
        }
        else
        {
            // Error if there are unparsed tokens left
            throw SqlParsingExceptionBuilder.Interrupted(tokenizer);
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
        var tableSource = TableSourceParser.Parse(tokenizer);

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
}
