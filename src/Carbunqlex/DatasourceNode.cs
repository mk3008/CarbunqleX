using Carbunqlex.QuerySources;
using System.Collections.ObjectModel;

namespace Carbunqlex;

/// <summary>
/// Types of data sources.
/// </summary>
public enum DatasourceType
{
    /// <summary>
    /// Table.
    /// </summary>
    Table,
    /// <summary>
    /// Subquery.
    /// </summary>
    SubQuery,
    /// <summary>
    /// CTE.
    /// </summary>
    CommonTableExtension,
    /// <summary>
    /// UNION query.
    /// </summary>
    UnionSubQuery
}

public class DatasourceNode
{
    /// <summary>
    /// Data source.
    /// </summary>
    private DatasourceExpression Expression { get; }

    /// <summary>
    /// Alias of the data source.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Type of the data source.
    /// </summary>
    public DatasourceType DatasourceType { get; }

    /// <summary>
    /// Full name of the data source.
    /// </summary>
    public string TableFullName { get; }

    /// <summary>
    /// Column names owned by the data source.
    /// </summary>
    public ReadOnlyDictionary<string, string> Columns { get; }

    /// <summary>
    /// Child query nodes that make up the data source.
    /// Stored here in case of subqueries or CTEs.
    /// </summary>
    public IReadOnlyList<QueryNode> ChildQueryNodes { get; }

    public DatasourceNode(DatasourceExpression expression, DatasourceType datasourceType, IEnumerable<string> columns, IEnumerable<QueryNode> childQueryNodes)
    {
        Expression = expression;
        DatasourceType = datasourceType;
        Name = Expression.Alias.ToLowerInvariant();
        TableFullName = Expression.Datasource.TableFullName;
        Columns = columns.ToDictionary(static column => column.ToLowerInvariant(), static column => column).AsReadOnly();
        ChildQueryNodes = childQueryNodes?.ToList().AsReadOnly() ?? new List<QueryNode>().AsReadOnly();
    }
}
