using Carbunqlex.Clauses;
using Carbunqlex.Parsing.ValueExpression;

namespace Carbunqlex.Parsing;

public static class DeleteQueryParser
{
    public static DeleteQuery Parse(string sql)
    {
        return Parse(new SqlTokenizer(sql));
    }

    public static DeleteQuery Parse(SqlTokenizer tokenizer)
    {
        // Parse the WITH clause
        WithClause? withClause = null;
        if (tokenizer.Peek().CommandOrOperatorText == "with")
        {
            withClause = WithClauseParser.Parse(tokenizer);
        }
        withClause ??= new();

        // Parse the DELETE clause
        var deleteClause = ParseDeleteClause(tokenizer);

        if (tokenizer.IsEnd)
        {
            return new DeleteQuery(withClause, deleteClause, null, null, null);
        }

        // Parse the optional USING clause
        UsingClause? usingClause = null;
        if (tokenizer.Peek().CommandOrOperatorText == "using")
        {
            usingClause = UsingClauseParser.Parse(tokenizer);
        }

        // Parse the optional WHERE clause
        WhereClause? whereClause = null;
        if (tokenizer.Peek().CommandOrOperatorText == "where")
        {
            whereClause = WhereClauseParser.Parse(tokenizer);
        }

        if (tokenizer.IsEnd)
        {
            return new DeleteQuery(withClause, deleteClause, usingClause, whereClause, null);
        }

        // Parse the optional RETURNING clause
        ReturningClause? returningClause = null;
        if (tokenizer.Peek().CommandOrOperatorText == "returning")
        {
            returningClause = ReturningClauseParser.Parse(tokenizer);
        }

        if (tokenizer.IsEnd)
        {
            return new DeleteQuery(withClause, deleteClause, usingClause, whereClause, returningClause);
        }

        // Error if there are unparsed tokens left
        throw SqlParsingExceptionBuilder.Interrupted(tokenizer, new DeleteQuery(withClause, deleteClause, usingClause, whereClause, returningClause).ToSql());
    }

    private static DeleteClause ParseDeleteClause(SqlTokenizer tokenizer)
    {
        tokenizer.Read("delete from");

        // Parse the table source
        var tableSource = TableSourceParser.Parse(tokenizer);

        // Parse the optional alias
        string alias = string.Empty;
        if (tokenizer.TryPeek(out var asToken) && asToken.CommandOrOperatorText == "as")
        {
            tokenizer.CommitPeek();
            alias = tokenizer.Read().Value;
        }
        else if (tokenizer.TryPeek(out var aliasToken) && aliasToken.Type == TokenType.Identifier)
        {
            alias = tokenizer.Read().Value;
        }

        return new DeleteClause(tableSource, alias);
    }
}
