using System.Diagnostics.CodeAnalysis;

namespace Carbunqlex.DatasourceExpressions;

public class UnionQuerySource : IDatasource
{
    public ISelectQuery Query { get; set; }

    public string Alias { get; set; }

    public string TableFullName => string.Empty;

    public List<string> ColumnNames { get; } = new();

    public UnionQuerySource(ISelectQuery query, string alias, IEnumerable<string> columns)
    {
        Query = query;
        Alias = alias;
        ColumnNames = columns.ToList();
    }

    public string ToSqlWithoutCte()
    {
        return Query.ToSqlWithoutCte();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        return Query.GenerateTokensWithoutCte();
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        yield return Query;
    }

    public IEnumerable<string> GetSelectableColumns()
    {
        return Query.GetSelectExpressions().Select(column => column.Alias);
    }

    public bool TryGetSubQuery([NotNullWhen(true)] out ISelectQuery? subQuery)
    {
        subQuery = null;
        return false;
    }

    public bool TryGetTableName([NotNullWhen(true)] out string? tableFullName)
    {
        tableFullName = null;
        return false;
    }

    public bool TryGetUnionQuerySource([NotNullWhen(true)] out UnionQuerySource? unionQuerySource)
    {
        unionQuerySource = this;
        return true;
    }
}
