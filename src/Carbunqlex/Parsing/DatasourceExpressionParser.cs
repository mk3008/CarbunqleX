using Carbunqlex.DatasourceExpressions;

namespace Carbunqlex.Parsing;

/// <summary>
/// Parses datasource expressions from SQL tokens.
/// e.g. [dbo].[table] as alias
/// </summary>
public class DatasourceExpressionParser
{
    private static string ParserName => nameof(DatasourceExpressionParser);

    public static DatasourceExpression Parse(SqlTokenizer tokenizer)
    {
        var datasource = DatasourceParser.Parse(tokenizer);

        // No alias
        if (tokenizer.IsEnd)
        {
            return new DatasourceExpression(datasource);
        }

        var next = tokenizer.Peek();

        if (next.Identifier == "as")
        {
            // Alias with AS keyword
            tokenizer.CommitPeek();
            return ParseWithAlias(tokenizer, datasource);
        }

        if (next.Type == TokenType.Identifier)
        {
            // Alias without AS keyword
            return ParseWithAlias(tokenizer, datasource);
        }

        // No alias
        return new DatasourceExpression(datasource);
    }

    private static DatasourceExpression ParseWithAlias(SqlTokenizer tokenizer, IDatasource datasource)
    {
        var alias = tokenizer.Read().Value;

        if (tokenizer.IsEnd)
        {
            // No column alias clause and no condition
            return new DatasourceExpression(datasource, alias);
        }

        if (tokenizer.Peek().Type == TokenType.OpenParen)
        {
            // Column alias clause present
            var columnAliasClause = ColumnAliasClauseParser.Parse(tokenizer);
            return new DatasourceExpression(datasource, alias, columnAliasClause);
        }

        // No column alias clause
        return new DatasourceExpression(datasource, alias);
    }
}
