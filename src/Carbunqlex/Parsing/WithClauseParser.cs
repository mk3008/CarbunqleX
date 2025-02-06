using Carbunqlex.Clauses;

namespace Carbunqlex.Parsing;

public static class WithClauseParser
{
    public static WithClause Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read("with");
        var commonTables = ParseCommonTables(tokenizer);
        return new WithClause(commonTables);
    }

    public static List<CommonTableClause> ParseCommonTables(SqlTokenizer tokenizer)
    {
        var commonTables = new List<CommonTableClause>();
        while (true)
        {
            commonTables.Add(ParseCommonTable(tokenizer));
            if (tokenizer.IsEnd || tokenizer.Peek().CommandOrOperatorText != ",")
            {
                break;
            }
            tokenizer.Read(TokenType.Comma);
        }
        return commonTables;
    }

    private static CommonTableClause ParseCommonTable(SqlTokenizer tokenizer)
    {
        bool isRecursive = tokenizer.Peek(static (r, t) =>
        {
            if (t.CommandOrOperatorText == "recursive")
            {
                r.CommitPeek();
                return true;
            }
            return false;
        });

        var name = tokenizer.Read(TokenType.Identifier);

        ColumnAliasClause? columnAliases = tokenizer.Peek(static (r, t) =>
        {
            if (t.CommandOrOperatorText == "(")
            {
                return ColumnAliasClauseParser.Parse(r);
            }
            return null;
        });

        tokenizer.Read("as");

        bool? isMaterialized = tokenizer.Peek(static (r, t) =>
        {
            if (t.CommandOrOperatorText == "materialized")
            {
                r.CommitPeek();
                return (bool?)true;
            }
            if (t.CommandOrOperatorText == "not materialized")
            {
                r.CommitPeek();
                return false;
            }
            return null;
        });

        tokenizer.Read(TokenType.OpenParen);
        var query = SelectQueryParser.Parse(tokenizer);
        tokenizer.Read(TokenType.CloseParen);

        return new CommonTableClause(query, name.Value, columnAliases, isMaterialized, isRecursive);
    }
}
