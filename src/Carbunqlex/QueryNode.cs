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
    public ISelectQuery Query { get; private set; }

    /// <summary>
    /// The column names selected by the query.
    /// </summary>
    private ReadOnlyDictionary<string, SelectExpression> SelectExpressionMap { get; set; }

    private bool MustRefresh { get; set; }

    /// <summary>
    /// The datasource nodes that make up the query.
    /// </summary>
    internal ReadOnlyDictionary<string, DatasourceNode> DatasourceNodeMap { get; private set; }

    public QueryNode(ISelectQuery query, IEnumerable<DatasourceNode> datasourceNodes)
    {
        Query = query;
        SelectExpressionMap = query.GetSelectExpressions().ToDictionary(static expr => expr.Alias.ToLowerInvariant(), expr => expr).AsReadOnly();
        DatasourceNodeMap = datasourceNodes.ToDictionary(static ds => ds.Name.ToLowerInvariant(), static ds => ds).AsReadOnly();
        MustRefresh = false;
    }

    /// <summary>
    /// Generates a string representing the state of the query node.
    /// </summary>
    /// <returns>A string representing the state of the query node.</returns>
    public string ToTreeString()
    {
        if (MustRefresh) Refresh();

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
        sb.AppendLine($"{indent} SelectedColumns: {string.Join(", ", SelectExpressionMap.Select(x => x.Key))}");

        indentLevel++;
        indent = new string(' ', indentLevel * 2);

        foreach (var datasourceNode in DatasourceNodeMap)
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

    public QueryNode SelectModifier(string columnName, Action<SelectModifier> action)
    {
        if (MustRefresh) Refresh();

        var result = GetColumnModifiers(columnName, isSelectableOnly: true);

        foreach (var columnModifier in result)
        {
            var selectModifier = new SelectModifier(columnModifier);
            action(selectModifier);
        }

        if (result.Any()) MustRefresh = true;

        return this;
    }

    public QueryNode WhereModifier(string columnName, Action<WhereModifier> action)
    {
        if (MustRefresh) Refresh();

        var result = GetColumnModifiers(columnName, isSelectableOnly: false);

        foreach (var columnModifier in result)
        {
            var whereModifier = new WhereModifier(columnModifier);
            action(whereModifier);
        }

        if (result.Any()) MustRefresh = true;

        return this;
    }

    public QueryNode JoinModifier(IEnumerable<string> columnNames, Action<JoinModifier> action)
    {
        if (MustRefresh) Refresh();

        var result = GetJoinModifiers(columnNames);

        foreach (var joinModifier in result)
        {
            action(joinModifier);
        }

        if (result.Any()) MustRefresh = true;

        return this;
    }

    private List<ColumnModifier> GetColumnModifiers(string columnName, bool isSelectableOnly)
    {
        var result = new List<ColumnModifier>();
        WhenRecursive(this, columnName.ToLowerInvariant(), isSelectableOnly, result);
        return result;
    }

    private List<JoinModifier> GetJoinModifiers(IEnumerable<string> columnNames)
    {
        var result = new List<JoinModifier>();
        WhenRecursive(this, columnNames.Select(c => c.ToLowerInvariant()).ToList(), result);
        return result;
    }

    /// <summary>
    /// Searches for queries that match the predicate and executes
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public QueryNode When(string columnName, bool isSelectableOnly, Action<ColumnModifier> action)
    {
        if (MustRefresh) Refresh();

        var column = columnName.ToLowerInvariant();

        var result = new List<ColumnModifier>();
        WhenRecursive(this, column, isSelectableOnly, result);
        foreach (var item in result)
        {
            action(item);
        }

        if (result.Any()) MustRefresh = true;

        return this;
    }

    private void WhenRecursive(QueryNode node, string columnName, bool isSelectableOnly, List<ColumnModifier> result)
    {
        // Search child nodes first
        foreach (var datasourceNode in node.DatasourceNodeMap.Values)
        {
            foreach (var childQueryNode in datasourceNode.ChildQueryNodes)
            {
                WhenRecursive(childQueryNode, columnName, isSelectableOnly, result);
            }
        }

        if (result.Any())
        {
            return;
        }

        // query で使用されている列名を検索
        if (node.SelectExpressionMap.ContainsKey(columnName))
        {
            result.Add(new(node.Query, node.SelectExpressionMap[columnName].Value));
            return;
        }

        if (isSelectableOnly)
        {
            return;
        }

        // datasource で定義されている列名を検索
        var column = node.DatasourceNodeMap.Values.Where(ds => ds.Columns.ContainsKey(columnName)).FirstOrDefault();
        if (column != null)
        {
            var expr = new ColumnExpression(column.Name, column.Columns[columnName]);
            result.Add(new(node.Query, expr));
            return;
        }
    }


    private void WhenRecursive(QueryNode node, IList<string> columnNames, List<JoinModifier> result)
    {
        // Search child nodes first
        foreach (var datasourceNode in node.DatasourceNodeMap.Values)
        {
            foreach (var childQueryNode in datasourceNode.ChildQueryNodes)
            {
                WhenRecursive(childQueryNode, columnNames, result);
            }
        }

        if (result.Any())
        {
            return;
        }

        var values = new Dictionary<string, IValueExpression>();

        // query で使用されているか検索
        foreach (var columnName in columnNames)
        {
            if (node.SelectExpressionMap.ContainsKey(columnName))
            {
                values.Add(columnName.ToLowerInvariant(), node.SelectExpressionMap[columnName].Value);

            }
            else if (node.DatasourceNodeMap.Values.Where(ds => ds.Columns.ContainsKey(columnName)).Any())
            {
                var datasource = node.DatasourceNodeMap.Values.Where(ds => ds.Columns.ContainsKey(columnName)).First();
                values.Add(columnName.ToLowerInvariant(), new ColumnExpression(datasource.Name, datasource.Columns[columnName]));
            }
            else
            {
                // 1つでも見つからなかったら終了
                return;
            }
        }

        // 全ての列が見つかったら結果に追加
        result.Add(new(node.Query, values));
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

    public void AddJoin(JoinClause joinClause)
    {
        Query.AddJoin(joinClause);
    }

    private void Refresh()
    {
        var node = QueryNodeFactory.Create(Query);
        Query = node.Query;
        SelectExpressionMap = node.SelectExpressionMap;
        DatasourceNodeMap = node.DatasourceNodeMap;

        MustRefresh = false;
    }
}
