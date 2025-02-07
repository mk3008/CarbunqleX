using Carbunqlex.Clauses;
using Carbunqlex.ValueExpressions;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Carbunqlex.DatasourceExpressions;

public class DatasourceExpression : ISqlComponent, IDatasource
{
    public DatasourceExpression(IDatasource datasource)
    {
        Datasource = datasource;
        Alias = datasource.DefaultName;
        ColumnAliasClause = null;
    }

    public DatasourceExpression(IDatasource datasource, string alias)
    {
        Datasource = datasource;
        Alias = alias;
        ColumnAliasClause = null;
    }

    public DatasourceExpression(IDatasource datasource, string alias, IColumnAliasClause columnAliasClause)
    {
        Datasource = datasource;
        Alias = alias;
        ColumnAliasClause = columnAliasClause;
    }

    public DatasourceExpression(IDatasource datasource, string alias, IEnumerable<string> columnAlias)
    {
        Datasource = datasource;
        Alias = alias;
        ColumnAliasClause = new ColumnAliasClause(columnAlias);
    }

    public IDatasource Datasource { get; }

    /// <summary>
    /// The alias of the datasource.
    /// </summary>
    public string Alias { get; }

    public IColumnAliasClause? ColumnAliasClause { get; set; }

    public TableSample? TableSample { get; set; }

    public string DefaultName => Datasource.DefaultName;

    public string TableFullName => Datasource.TableFullName;

    public string ToSqlWithoutCte()
    {
        if (string.IsNullOrWhiteSpace(Alias))
        {
            return Datasource.ToSqlWithoutCte();
        }

        if (Alias == Datasource.DefaultName && ColumnAliasClause == null)
        {
            return Datasource.ToSqlWithoutCte();
        }

        var sb = new StringBuilder();
        sb.Append(Datasource.ToSqlWithoutCte());
        sb.Append(" as ");
        sb.Append(Alias);

        if (ColumnAliasClause == null)
        {
            return sb.ToString();
        }

        sb.Append(ColumnAliasClause.ToSqlWithoutCte());
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        if (string.IsNullOrWhiteSpace(Alias))
        {
            return Datasource.GenerateTokensWithoutCte();
        }

        if (Alias == Datasource.DefaultName && ColumnAliasClause == null)
        {
            return Datasource.GenerateTokensWithoutCte();
        }

        var tokens = new List<Token>();
        tokens.AddRange(Datasource.GenerateTokensWithoutCte());
        tokens.Add(new Token(TokenType.Command, "as"));
        tokens.Add(new Token(TokenType.Identifier, Alias));

        if (ColumnAliasClause == null)
        {
            return tokens;
        }

        tokens.AddRange(ColumnAliasClause.GenerateTokensWithoutCte());
        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return Datasource.GetQueries();
    }

    public IEnumerable<ColumnExpression> GetSelectableColumnExpressions()
    {
        if (ColumnAliasClause != null)
        {
            return ColumnAliasClause.GetColumnNames().Select(columnName => new ColumnExpression(Alias, columnName));
        }

        return Datasource.GetSelectableColumns().Select(columnName => new ColumnExpression(Alias, columnName));
    }

    public IEnumerable<string> GetSelectableColumns()
    {
        if (ColumnAliasClause != null)
        {
            return ColumnAliasClause.GetColumnNames();
        }

        return Datasource.GetSelectableColumns();
    }

    public bool TryGetSubQuery([NotNullWhen(true)] out ISelectQuery? subQuery)
    {
        return Datasource.TryGetSubQuery(out subQuery);
    }

    public bool TryGetTableName([NotNullWhen(true)] out string? tableFullName)
    {
        return Datasource.TryGetTableName(out tableFullName);
    }

    public bool TryGetUnionQuerySource([NotNullWhen(true)] out UnionQuerySource? unionQuerySource)
    {
        return Datasource.TryGetUnionQuerySource(out unionQuerySource);
    }
}

public class TableSample : ISqlComponent
{
    public TableSample(string sampleType, IValueExpression sampleCount)
    {
        SampleType = sampleType;
        SampleCount = sampleCount;
    }

    /// <summary>
    /// The type of sample.
    /// e.g. BERNOULLI or SYSTEM
    /// </summary>
    public string SampleType { get; }

    public IValueExpression SampleCount { get; }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder("tablesample (");
        sb.Append(SampleCount.ToSqlWithoutCte());
        sb.Append(SampleType).Append(')');

        return sb.ToString();
    }
    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>
        {
            new Token(TokenType.Command, "tablesample"),
            new Token(TokenType.OpenParen, "("),
        };
        tokens.AddRange(SampleCount.GenerateTokensWithoutCte());
        tokens.Add(new Token(TokenType.Command, SampleType));
        tokens.Add(new Token(TokenType.CloseParen, ")"));
        return tokens;
    }
    public IEnumerable<ISelectQuery> GetQueries()
    {
        yield break;
    }
}
