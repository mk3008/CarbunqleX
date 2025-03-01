using System.Diagnostics.CodeAnalysis;

namespace Carbunqlex.QuerySources;

public interface IDatasource : ISqlComponent
{
    string DefaultName { get; }

    /// <summary>
    /// The full name of the table.
    /// </summary>
    string TableFullName { get; }

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
    bool TryGetSubQuery([NotNullWhen(true)] out ISelectQuery? subQuery);

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
