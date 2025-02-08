using Carbunqlex.DatasourceExpressions;

namespace Carbunqlex.Parsing;

/// <summary>
/// Parses datasource expressions from SQL tokens.
/// e.g. [dbo].[table] as alias
/// </summary>
public class DatasourceExpressionParser
{
    public static DatasourceExpression Parse(SqlTokenizer tokenizer)
    {
        var datasource = DatasourceParser.Parse(tokenizer);

        // No alias
        if (tokenizer.IsEnd)
        {
            return new DatasourceExpression(datasource);
        }

        var next = tokenizer.Peek();

        // Alias keyword pattern
        // [dbo].[table] as alias
        // [dbo].[table] alias
        // [dbo].[table]
        var expression = next.CommandOrOperatorText == "as"
            ? ParseWithAliasKeyword(tokenizer, datasource)
            : next.Type == TokenType.Identifier
                ? ParseWithOutAliasKeyword(tokenizer, datasource)
                : ParseNoAlias(tokenizer, datasource);

        if (tokenizer.IsEnd)
        {
            return expression;
        }

        // table sample clause
        var nextToken = tokenizer.Peek();
        if (tokenizer.Peek().CommandOrOperatorText == "tablesample")
        {
            expression.TableSample = ParseTableSample(tokenizer);
            return expression;
        }

        return expression;
    }

    private static DatasourceExpression ParseNoAlias(SqlTokenizer tokenizer, IDatasource datasource)
    {
        if (tokenizer.IsEnd)
        {
            // No column alias clause and no condition
            return new DatasourceExpression(datasource);
        }
        // No column alias clause
        return new DatasourceExpression(datasource);
    }

    private static DatasourceExpression ParseWithAliasKeyword(SqlTokenizer tokenizer, IDatasource datasource)
    {
        tokenizer.Read("as");
        return ParseWithOutAliasKeyword(tokenizer, datasource);
    }

    private static DatasourceExpression ParseWithOutAliasKeyword(SqlTokenizer tokenizer, IDatasource datasource)
    {
        var alias = tokenizer.Read(TokenType.Identifier).Value;

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

    private static TableSample ParseTableSample(SqlTokenizer tokenizer)
    {
        tokenizer.Read("tablesample");

        var system = tokenizer.Read(TokenType.Command).Value;

        tokenizer.Read(TokenType.OpenParen);
        var value = ValueExpressionParser.Parse(tokenizer);
        tokenizer.Read(TokenType.CloseParen);

        return new TableSample(system, value);
    }
}
