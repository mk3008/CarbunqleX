using Carbunqlex.Clauses;
using System.Collections.ObjectModel;
using System.Text;

namespace Carbunqlex;

public class QueryNode
{
    /// <summary>
    /// The query.
    /// </summary>
    public IQuery Query { get; }

    /// <summary>
    /// The column names selected by the query.
    /// </summary>
    private ReadOnlyDictionary<string, SelectExpression> SelectExpressions { get; }

    /// <summary>
    /// The datasource nodes that make up the query.
    /// </summary>
    internal ReadOnlyDictionary<string, DatasourceNode> DatasourceNodes { get; }

    public QueryNode(IQuery query, IEnumerable<DatasourceNode> datasourceNodes)
    {
        Query = query;
        SelectExpressions = query.GetSelectExpressions().ToDictionary(static expr => expr.Alias.ToLowerInvariant(), expr => expr).AsReadOnly();
        DatasourceNodes = datasourceNodes.ToDictionary(static ds => ds.Name.ToLowerInvariant(), static ds => ds).AsReadOnly();
    }

    /// <summary>
    /// Generates a string representing the state of the query node.
    /// </summary>
    /// <returns>A string representing the state of the query node.</returns>
    public string ToTreeString()
    {
        var sb = new StringBuilder();
        AppendTreeString(sb, 0);
        return sb.ToString();
    }

    private void AppendTreeString(StringBuilder sb, int indentLevel)
    {
        var indent = new string(' ', indentLevel * 2);
        sb.AppendLine($"{indent}*Query");
        sb.AppendLine($"{indent} Type: {Query.GetType().Name}");
        sb.AppendLine($"{indent} Current: {Query.ToSqlWithoutCte()}");
        sb.AppendLine($"{indent} SelectedColumns: {string.Join(", ", SelectExpressions.Select(x => x.Key))}");

        indentLevel++;
        indent = new string(' ', indentLevel * 2);

        foreach (var datasourceNode in DatasourceNodes)
        {
            sb.AppendLine($"{indent}*Datasource");
            sb.AppendLine($"{indent} Type: {datasourceNode.Value.DatasourceType}");
            sb.AppendLine($"{indent} Name: {datasourceNode.Value.Name}");
            sb.AppendLine($"{indent} Table: {datasourceNode.Value.TableFullName}");
            sb.AppendLine($"{indent} Columns: {string.Join(", ", datasourceNode.Value.Columns.Select(x => x.Key))}");

            foreach (var childQueryNode in datasourceNode.Value.ChildQueryNodes)
            {
                childQueryNode.AppendTreeString(sb, indentLevel + 1);
            }
        }
    }
}
