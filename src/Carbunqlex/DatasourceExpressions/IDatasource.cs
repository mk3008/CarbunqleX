using Carbunqlex.ValueExpressions;
using System.Diagnostics.CodeAnalysis;

namespace Carbunqlex.DatasourceExpressions;

public interface IDatasource : ISqlComponent
{
    /// <summary>
    /// The alias of the datasource.
    /// </summary>
    string Alias { get; }

    /// <summary>
    /// Retrieves the selectable columns.
    /// </summary>
    /// <returns></returns>
    IEnumerable<string> GetSelectableColumns();

    /// <summary>
    /// Retrieves the sub query if it exists.
    /// </summary>
    /// <param name="subSuqery"></param>
    /// <returns></returns>
    bool TryGetSubQuery([NotNullWhen(true)] out IQuery? subQuery);

    /// <summary>
    /// Retrieves the table name.
    /// </summary>
    /// <returns></returns>
    bool TryGetTableName([NotNullWhen(true)] out string? tableFullName);

    /// <summary>
    /// Retrieves the union query if it exists.
    /// </summary>
    /// <param name="unionQuerySource"></param>
    /// <returns></returns>
    bool TryGetUnionQuerySource([NotNullWhen(true)] out UnionQuerySource? unionQuerySource);
}

public static class IDatasourceExtensions
{
    public static IEnumerable<ColumnExpression> GetSelectableColumnExpressions(this IDatasource datasource)
    {
        return datasource.GetSelectableColumns().Select(columnName => new ColumnExpression(datasource.Alias, columnName));
    }
}
