using Carbunqlex.Clauses;
using Carbunqlex.Parsing.ValueExpression;

namespace Carbunqlex.Parsing;

public static class UpdateQueryParser
{
    public static UpdateQuery Parse(string sql)
    {
        return Parse(new SqlTokenizer(sql));
    }

    public static UpdateQuery Parse(SqlTokenizer tokenizer)
    {
        var withClause = tokenizer.Peek(static (r, t) =>
        {
            if (t.CommandOrOperatorText == "with")
            {
                return WithClauseParser.Parse(r);
            }
            return new WithClause();
        }, new WithClause());

        var updateClause = UpdateClauseParser.Parse(tokenizer);

        var setClause = SetClauseParser.Parse(tokenizer);

        var fromClause = tokenizer.Peek(static (r, t) =>
        {
            if (t.CommandOrOperatorText == "from")
            {
                return (IFromClause)FromClauseParser.Parse(r);
            }
            return EmptyFromClause.Instance;
        }, EmptyFromClause.Instance);

        var whereClause = tokenizer.Peek(static (r, t) =>
        {
            if (t.CommandOrOperatorText == "where")
            {
                return WhereClauseParser.Parse(r);
            }
            return new WhereClause();
        }, new WhereClause());

        var returning = tokenizer.Peek(static (r, t) =>
        {
            if (t.CommandOrOperatorText == "returning")
            {
                return ReturningClauseParser.Parse(r);
            }
            return null;
        }, null);

        if (!tokenizer.IsEnd)
        {
            throw SqlParsingExceptionBuilder.Interrupted(tokenizer);
        }
        return new UpdateQuery(withClause, updateClause, setClause, fromClause, whereClause, returning);
    }

    private class UpdateClauseParser
    {
        public static UpdateClause Parse(SqlTokenizer tokenizer)
        {
            tokenizer.Read("update");
            var tableName = TableDatasourceParser.Parse(tokenizer);
            var alias = string.Empty;
            if (tokenizer.Peek().CommandOrOperatorText == "as")
            {
                tokenizer.CommitPeek();
                alias = tokenizer.Read(TokenType.Identifier).Value;
            }
            else if (tokenizer.Peek().Type == TokenType.Identifier)
            {
                // alias without AS keyword
                alias = tokenizer.Read(TokenType.Identifier).Value;
            }
            return new UpdateClause(tableName, alias);
        }
    }

    private static class SetClauseParser
    {
        public static SetClause Parse(SqlTokenizer tokenizer)
        {
            tokenizer.Read("set");
            var setExpressions = new List<SetExpression>(){
                SetExpressionParser.Parse(tokenizer)
            };
            while (true)
            {
                if (tokenizer.Peek().Type == TokenType.Comma)
                {
                    tokenizer.CommitPeek();
                    setExpressions.Add(SetExpressionParser.Parse(tokenizer));
                }
                break;
            }
            return new SetClause(setExpressions);
        }
    }

    private static class SetExpressionParser
    {
        public static SetExpression Parse(SqlTokenizer tokenizer)
        {
            var column = tokenizer.Read(TokenType.Identifier).Value;
            tokenizer.Read("=");
            var value = ValueExpressionParser.Parse(tokenizer);
            return new SetExpression(column, value);
        }
    }
}
