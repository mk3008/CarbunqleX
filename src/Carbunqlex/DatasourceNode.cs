using Carbunqlex.DatasourceExpressions;
using System.Collections.ObjectModel;

namespace Carbunqlex;

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
    /// データソースが所持している列名。
    /// </summary>
    public ReadOnlyDictionary<string, string> Columns { get; }

    /// <summary>
    /// データソースを構成している子クエリノード。
    /// サブクエリ、CTEの場合はここに格納される。
    /// </summary>
    public IReadOnlyList<QueryNode> ChildQueryNodes { get; }

    public DatasourceNode(IDatasource datasource, IEnumerable<string> columns, IEnumerable<QueryNode> childQueryNodes)
    {
        Datasource = datasource;
        Name = Datasource.Alias.ToLowerInvariant();
        Columns = columns.ToDictionary(static column => column.ToLowerInvariant(), static column => column).AsReadOnly();
        ChildQueryNodes = childQueryNodes?.ToList().AsReadOnly() ?? new List<QueryNode>().AsReadOnly();
    }
}
