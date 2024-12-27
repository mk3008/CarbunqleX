using Carbunqlex.DatasourceExpressions;
using System.Collections.ObjectModel;

namespace Carbunqlex;

/// <summary>
/// データソースの種類。
/// </summary>
public enum DatasourceType
{
    /// <summary>
    /// テーブル。
    /// </summary>
    Table,
    /// <summary>
    /// サブクエリ。
    /// </summary>
    SubQuery,
    /// <summary>
    /// CTE。
    /// </summary>
    CommonTableExtension,
    /// <summary>
    /// UNIONクエリ。
    /// </summary>
    UnionSubQuery
}

public class DatasourceNode
{
    /// <summary>
    /// データソース。
    /// </summary>
    private IDatasource Datasource { get; }

    /// <summary>
    /// データソースのエイリアス。
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// データソースの種類。
    /// </summary>
    public DatasourceType DatasourceType { get; }

    /// <summary>
    /// データソースのフルネーム。
    /// </summary>
    public string TableFullName { get; }

    /// <summary>
    /// データソースが所持している列名。
    /// </summary>
    public ReadOnlyDictionary<string, string> Columns { get; }

    /// <summary>
    /// データソースを構成している子クエリノード。
    /// サブクエリ、CTEの場合はここに格納される。
    /// </summary>
    public IReadOnlyList<QueryNode> ChildQueryNodes { get; }

    public DatasourceNode(IDatasource datasource, DatasourceType datasourceType, IEnumerable<string> columns, IEnumerable<QueryNode> childQueryNodes)
    {
        Datasource = datasource;
        DatasourceType = datasourceType;
        Name = Datasource.Alias.ToLowerInvariant();
        TableFullName = Datasource.TableFullName;
        Columns = columns.ToDictionary(static column => column.ToLowerInvariant(), static column => column).AsReadOnly();
        ChildQueryNodes = childQueryNodes?.ToList().AsReadOnly() ?? new List<QueryNode>().AsReadOnly();
    }
}


