using Carbunqlex.Clauses;
using Carbunqlex.DatasourceExpressions;
using Carbunqlex.ValueExpressions;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Carbunqlex;

public class QueryNode : ISelectQuery
{
    /// <summary>
    /// The query.
    /// </summary>
    public ISelectQuery Query { get; }

    /// <summary>
    /// The column names selected by the query.
    /// </summary>
    private ReadOnlyDictionary<string, SelectExpression> SelectExpressions { get; }

    /// <summary>
    /// The datasource nodes that make up the query.
    /// </summary>
    internal ReadOnlyDictionary<string, DatasourceNode> DatasourceNodes { get; }

    public QueryNode(ISelectQuery query, IEnumerable<DatasourceNode> datasourceNodes)
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

    /// <summary>
    /// Searches for queries that match the predicate and executes
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public QueryNode When(string columnName, Action<ColumnModifier> action)
    {
        var column = columnName.ToLowerInvariant();

        var result = new List<ColumnModifier>();
        WhenRecursive(this, column, result);
        foreach (var item in result)
        {
            action(item);
        }
        return this;
    }

    private void WhenRecursive(QueryNode node, string columnName, List<ColumnModifier> result)
    {
        // Search child nodes first
        foreach (var datasourceNode in node.DatasourceNodes.Values)
        {
            foreach (var childQueryNode in datasourceNode.ChildQueryNodes)
            {
                WhenRecursive(childQueryNode, columnName, result);
            }
        }

        if (result.Any())
        {
            return;
        }

        // query で使用されている列名を検索
        if (node.SelectExpressions.ContainsKey(columnName))
        {
            result.Add(new(node.Query, node.SelectExpressions[columnName].Value));
            return;
        }

        // datasource で定義されている列名を検索
        var column = node.DatasourceNodes.Values.Where(ds => ds.Columns.ContainsKey(columnName)).FirstOrDefault();
        if (column != null)
        {
            var expr = new ColumnExpression(column.Name, column.Columns[columnName]);
            result.Add(new(node.Query, expr));
            return;
        }
    }

    public IEnumerable<SelectExpression> GetSelectExpressions()
    {
        return Query.GetSelectExpressions();
    }

    public IEnumerable<IDatasource> GetDatasources()
    {
        return Query.GetDatasources();
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        return Query.ExtractColumnExpressions();
    }

    public bool TryGetWhereClause([NotNullWhen(true)] out WhereClause? whereClause)
    {
        return Query.TryGetWhereClause(out whereClause);
    }

    public void AddColumn(SelectExpression expr)
    {
        Query.AddColumn(expr);
    }

    public void AddColumn(IValueExpression value, string alias)
    {
        Query.AddColumn(value, alias);
    }

    public void RemoveColumn(SelectExpression expr)
    {
        Query.RemoveColumn(expr);
    }

    public string ToSql()
    {
        return Query.ToSql();
    }

    public IEnumerable<Lexeme> GenerateLexemes()
    {
        return Query.GenerateLexemes();
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        return Query.GetCommonTableClauses();
    }

    public IDictionary<string, object?> GetParameters()
    {
        return Query.GetParameters();
    }

    public ParameterExpression AddParameter(string name, object value)
    {
        return Query.AddParameter(name, value);
    }

    public string ToSqlWithoutCte()
    {
        return Query.ToSqlWithoutCte();
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        return Query.GenerateLexemesWithoutCte();
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return Query.GetQueries();
    }
}
