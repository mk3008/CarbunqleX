using System.Diagnostics.CodeAnalysis;

namespace Carbunqlex.DatasourceExpressions;

public class UnionQuerySource : IDatasource
{
    public IQuery Query { get; set; }

    public string Alias { get; set; }

    public List<string> ColumnNames { get; } = new();

    public UnionQuerySource(IQuery query, string alias, IEnumerable<string> columns)
    {
        Query = query;
        Alias = alias;
        ColumnNames = columns.ToList();
    }

    public string ToSqlWithoutCte()
    {
        return Query.ToSqlWithoutCte();
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        return Query.GenerateLexemesWithoutCte();
    }

    public IEnumerable<IQuery> GetQueries()
    {
        yield return Query;
    }

    public IEnumerable<string> GetSelectableColumns()
    {
        return Query.GetSelectExpressions().Select(column => column.Alias);
    }

    public bool TryGetSubQuery([NotNullWhen(true)] out IQuery? subQuery)
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
