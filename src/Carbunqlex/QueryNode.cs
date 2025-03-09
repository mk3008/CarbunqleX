using Carbunqlex.Clauses;
using Carbunqlex.Editors;
using Carbunqlex.Expressions;
using Carbunqlex.Parsing.Expressions;
using Carbunqlex.Parsing.QuerySources;
using Carbunqlex.QuerySources;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("Carbunqlex.Tests")]

namespace Carbunqlex;

/// <summary>
/// Represents a query node that can be used to modify a query.
/// </summary>
public class QueryNode : IQuery
{
    /// <summary>
    /// The query.
    /// </summary>
    public ISelectQuery Query { get; private set; }

    /// <summary>
    /// The column names selected by the query.
    /// </summary>
    internal ReadOnlyDictionary<string, SelectExpression> SelectExpressionMap { get; set; }

    internal bool MustRefresh { get; set; }

    /// <summary>
    /// The datasource nodes that make up the query.
    /// </summary>
    internal ReadOnlyDictionary<string, DatasourceNode> DatasourceNodeMap { get; private set; }

    public QueryNode(ISelectQuery query, IEnumerable<DatasourceNode> datasourceNodes)
    {
        Query = query;
        SelectExpressionMap = query.GetSelectExpressions().ToDictionary(static expr => expr.Alias.ToLowerInvariant(), expr => expr).AsReadOnly();

        // Group if the same data source is referenced multiple times
        DatasourceNodeMap = datasourceNodes
            .GroupBy(static ds => ds.Name.ToLowerInvariant())
            .ToDictionary(static g => g.Key, static g => g.First())
            .AsReadOnly();

        MustRefresh = false; MustRefresh = false;
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

        if (sb.Length > 0)
        {
            sb.Append("\r\n");
        }
        sb.Append($"{indent}*Query");
        sb.Append($"\r\n{indent} Type: {Query.GetType().Name}");
        sb.Append($"\r\n{indent} Current: {Query.ToSqlWithoutCte()}");
        sb.Append($"\r\n{indent} SelectedColumns: {string.Join(", ", SelectExpressionMap.Select(x => x.Key))}");

        indentLevel++;
        indent = new string(' ', indentLevel * 2);

        foreach (var datasourceNode in DatasourceNodeMap)
        {
            sb.Append($"\r\n{indent}*Datasource");
            sb.Append($"\r\n{indent} Type: {datasourceNode.Value.DatasourceType}");
            sb.Append($"\r\n{indent} Name: {datasourceNode.Value.Name}");
            sb.Append($"\r\n{indent} Table: {datasourceNode.Value.TableFullName}");
            sb.Append($"\r\n{indent} Columns: {string.Join(", ", datasourceNode.Value.Columns.Select(x => x.Key))}");

            foreach (var childQueryNode in datasourceNode.Value.ChildQueryNodes)
            {
                childQueryNode.AppendTreeString(sb, indentLevel + 1);
            }
        }
    }

    public UpdateQuery ToUpdateQuery(string tableName, IList<string> keyColumns, IList<string>? valueColumns = null, string subQueryAlias = "", bool hasReturning = false)
    {
        if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));

        if (MustRefresh) Refresh();

        var subAlias = string.IsNullOrEmpty(subQueryAlias) ? "q" : subQueryAlias;

        // get key columns
        var keys = SelectExpressionMap.Where(x => keyColumns.Contains(x.Key));
        if (keys.Count() != keyColumns.Count())
        {
            throw new InvalidOperationException("Key columns are not found in the subquery");
        }

        // get value columns
        var values = SelectExpressionMap.Where(x => !keyColumns.Contains(x.Key));
        if (valueColumns != null && values.Any())
        {
            values = values.Where(x => valueColumns.Contains(x.Key));
        }
        if (!values.Any())
        {
            throw new InvalidOperationException("No columns to update");
        }

        var updateClause = new UpdateClause(TableDatasourceParser.Parse(tableName));

        // from clause
        var datasource = new DatasourceExpression(new SubQuerySource(Query), subAlias);
        var fromClause = new FromClause(datasource);

        // add key columns to where clause
        var whereClause = new WhereClause();
        var keyBinaries = keys.Select(x => new BinaryExpression("=", new ColumnExpression(updateClause.Alias, x.Key), new ColumnExpression(datasource.Alias, x.Key))).ToList();
        keyBinaries.ForEach(whereClause.Add);

        // add non-key columns to set clause
        var setClause = new SetClause();
        var nonKeyBinaries = values.Select(x => new SetExpression(x.Key, new ColumnExpression(datasource.Alias, x.Key))).ToList();
        nonKeyBinaries.ForEach(setClause.Add);

        return new UpdateQuery(
            null,
            updateClause,
            setClause,
            fromClause,
            whereClause,
            hasReturning ? new ReturningClause() : null
        );
    }

    public CreateTableAsQuery ToCreateTableQuery(string tableName, bool isTemporary)
    {
        if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));

        if (MustRefresh) Refresh();

        var tableSource = TableDatasourceParser.Parse(tableName);
        return new CreateTableAsQuery(tableSource, Query, isTemporary);
    }

    public DeleteQuery ToDeleteQuery(string tableName, IEnumerable<string>? keyColumns = null, string subQueryAlias = "", bool hasReturning = false)
    {
        if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));

        if (MustRefresh) Refresh();

        var tableSource = TableDatasourceParser.Parse(tableName);
        var deleteClause = new DeleteClause(tableSource);

        var subAlias = string.IsNullOrEmpty(subQueryAlias) ? "q" : subQueryAlias;
        var sq = ToSubQuery(subAlias, keyColumns);

        var left = sq.SelectExpressionMap.Keys.Select(x => (IValueExpression)new ColumnExpression(deleteClause.Alias, x)).ToList();
        var right = new SubQueryExpression(sq.Query);

        var where = new WhereClause();
        where.Add(new InExpression(false, new InValueGroupExpression(left), right));

        if (!hasReturning)
        {
            return new DeleteQuery(new DeleteClause(tableSource), where);
        }
        else
        {
            // return all columns 
            return new DeleteQuery(null, new DeleteClause(tableSource), null, where, new ReturningClause());
        }
    }

    public InsertQuery ToInsertQuery(string tableName, IList<string>? sequenceColumns = null, IList<string>? valueColumns = null, string subQueryAlias = "", bool hasReturning = false)
    {
        if (MustRefresh) Refresh();

        var tableSource = TableDatasourceParser.Parse(tableName);

        // create subquery
        ISelectQuery query;
        var alias = string.IsNullOrEmpty(subQueryAlias) ? "q" : subQueryAlias;
        var columns = SelectExpressionMap.Select(x => x.Key);
        if (sequenceColumns != null && sequenceColumns.Any() && valueColumns != null && valueColumns.Any())
        {
            var allowedColumns = sequenceColumns.Concat(valueColumns);
            columns = columns.Where(allowedColumns.Contains);
            query = ToSubQuery(alias, columns).Query;
        }
        else if (valueColumns != null && valueColumns.Any())
        {
            columns = columns.Where(valueColumns.Contains);
            query = ToSubQuery(alias, columns).Query;
        }
        else
        {
            query = Query;
        }

        if (!columns.Any())
        {
            throw new InvalidOperationException("No columns to update");
        }

        if (!hasReturning)
        {
            return new InsertQuery(new InsertClause(tableSource, columns.ToList()), query);
        }
        else
        {
            if (sequenceColumns == null)
            {
                return new InsertQuery(new InsertClause(tableSource, columns.ToList()), query, new ReturningClause());

            }
            else
            {
                var sequenceColumnsExp = sequenceColumns.ToList().Select(x => new SelectExpression(new ColumnExpression(x)));
                var columnsExp = columns.ToList().Select(x => new SelectExpression(new ColumnExpression(x)));
                var exp = sequenceColumnsExp.Concat(columnsExp).Distinct().ToList();
                return new InsertQuery(new InsertClause(tableSource, columns.ToList()), query, new ReturningClause(exp));
            }
        }
    }

    public QueryNode ModifyColumn(string columnName, Action<SelectEditor> action)
    {
        if (MustRefresh) Refresh();

        var result = GetColumnEditors(columnName, isSelectableOnly: true, isCurrentOnly: false);

        foreach (var columnModifier in result)
        {
            var editor = new SelectEditor(columnModifier);
            action(editor);
        }

        if (result.Any()) MustRefresh = true;

        return this;
    }

    public QueryNode RemoveColumn(string columnName)
    {
        if (MustRefresh) Refresh();

        var result = GetColumnEditors(columnName, isSelectableOnly: true, isCurrentOnly: true).FirstOrDefault();
        if (result == null || result.SelectExpression == null)
        {
            return this;
        }

        Query.RemoveColumn(result.SelectExpression);
        MustRefresh = true;

        return this;
    }

    public QueryNode AddColumn(string columnExpression)
    {
        if (MustRefresh) Refresh();
        var result = GetColumnEditors(columnExpression, isSelectableOnly: true, isCurrentOnly: true).FirstOrDefault();
        if (result != null)
        {
            throw new InvalidOperationException("Column already selected");
        }

        Query.AddColumn(new SelectExpression(ColumnExpressionParser.Parse(columnExpression)));
        MustRefresh = true;
        return this;
    }

    public QueryNode AddColumn(string valueExpression, string alias)
    {
        if (MustRefresh) Refresh();
        var result = GetColumnEditors(alias, isSelectableOnly: true, isCurrentOnly: true).FirstOrDefault();
        if (result != null)
        {
            throw new InvalidOperationException("Alias already selected");
        }

        Query.AddColumn(new SelectExpression(ValueExpressionParser.Parse(valueExpression), alias));
        MustRefresh = true;
        return this;
    }

    public QueryNode From(string columnName, bool isCurrentOnly, Action<FromEditor> action)
    {
        return From(new List<string> { columnName }, isCurrentOnly, action);
    }

    public QueryNode From(IEnumerable<string> columnNames, bool isCurrentOnly, Action<FromEditor> action)
    {
        if (MustRefresh) Refresh();

        var result = GetFromEditors(columnNames, isCurrentOnly);

        foreach (var editor in result)
        {
            action(editor);
        }

        if (result.Any()) MustRefresh = true;

        return this;
    }

    /// <summary>
    /// Adds search conditions to the current query.
    /// </summary>
    /// <param name="conditionExpressionText"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public QueryNode Where(string conditionExpressionText)
    {
        /// 強制追加するため、事前Refreshは不要
        if (Query.TryGetWhereClause(out var whereClause))
        {
            whereClause.Add(ValueExpressionParser.Parse(conditionExpressionText));
            MustRefresh = true;
        }
        else
        {
            throw new NotSupportedException("The query must have a WHERE clause.");
        }
        return this;
    }

    /// <summary>
    /// Searches for queries that contain the specified column name and adds search conditions.
    /// If not found, does nothing.
    /// </summary>
    /// <param name="columnName"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public QueryNode Where(string columnName, Action<WhereEditor> action)
    {
        return Where(new List<string> { columnName }, action);
    }

    /// <summary>
    /// Searches for queries that contain the specified column names and adds search conditions.
    /// If not found, does nothing.
    /// </summary>
    /// <param name="columnNames"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public QueryNode Where(IEnumerable<string> columnNames, Action<WhereEditor> action)
    {
        if (MustRefresh) Refresh();

        var result = GetWhereEditors(columnNames);

        foreach (var editor in result)
        {
            action(editor);
        }

        if (result.Any()) MustRefresh = true;

        return this;
    }

    public QueryNode GroupBy(string columnName)
    {
        if (MustRefresh) Refresh();

        // The processing target is only the terminal SelectQuery
        if (Query is not SelectQuery sq)
        {
            throw new InvalidOperationException("GroupBy can only be used on a SelectQuery");
        }

        var lst = SelectExpressionMap.Where(x => x.Key == columnName.ToLowerInvariant()).Select(x => x.Value.Value).ToList();

        sq.GroupByClause.AddRange(lst);

        return this;
    }

    public QueryNode ToSubQuery(string alias = "d", IEnumerable<string>? selectColumns = null)
    {
        if (MustRefresh) Refresh();

        var selectExpressions = SelectExpressionMap.Select(x => new SelectExpression(new ColumnExpression(alias, x.Key)));

        if (selectColumns != null && selectColumns.Any())
        {
            selectExpressions = selectExpressions
                .Where(x => selectColumns.Contains(x.Alias, StringComparer.InvariantCultureIgnoreCase));
        }

        var sq = new SelectQuery(
            new SelectClause(selectExpressions.ToList()),
            new FromClause(new DatasourceExpression(new SubQuerySource(Query), alias))
            );

        return QueryAstParser.Parse(sq);
    }

    public QueryNode ToCteQuery(string cteName, string? alias = null, IEnumerable<string>? selectColumns = null, bool overrideNode = false)
    {
        if (MustRefresh) Refresh();

        var name = string.IsNullOrEmpty(alias) ? cteName : alias;

        var selectExpressions = SelectExpressionMap.Select(x => new SelectExpression(new ColumnExpression(name, x.Value.Alias)));

        if (selectColumns != null && selectColumns.Any())
        {
            selectExpressions = selectExpressions
                .Where(x => selectColumns.Contains(x.Alias, StringComparer.InvariantCultureIgnoreCase));
        }

        var commonTableClause = new CommonTableClause(Query, cteName);

        var sq = new SelectQuery(
            new SelectClause(selectExpressions.ToList()),
            new FromClause(new DatasourceExpression(new TableSource(name)))
            );
        sq.WithClause.Add(commonTableClause);

        if (overrideNode)
        {
            Query = sq;
            Refresh();
            return this;
        }

        return QueryAstParser.Parse(sq);
    }

    private List<ColumnEditor> GetColumnEditors(string columnName, bool isSelectableOnly, bool isCurrentOnly)
    {
        var result = new List<ColumnEditor>();
        WhenRecursive(this, columnName.ToLowerInvariant(), isSelectableOnly, result, isCurrentOnly);
        return result;
    }

    private List<FromEditor> GetFromEditors(IEnumerable<string> columnNames, bool isCurrentOnly)
    {
        var result = new List<FromEditor>();
        WhenRecursive(this, columnNames.Select(c => c.ToLowerInvariant()).ToList(), result, isCurrentOnly);
        return result;
    }

    private List<WhereEditor> GetWhereEditors(IEnumerable<string> columnNames)
    {
        var result = new List<WhereEditor>();
        WhenRecursive(this, columnNames.Select(c => c.ToLowerInvariant()).ToList(), result, isCurrentOnly: false);

        // Remove duplicates
        result = result.GroupBy(x => x.Query).Select(g => g.First()).ToList();

        return result;
    }

    /// <summary>
    /// Searches for queries that match the predicate and executes
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public QueryNode When(string columnName, bool isSelectableOnly, Action<ColumnEditor> action)
    {
        if (MustRefresh) Refresh();

        var column = columnName.ToLowerInvariant();

        var result = new List<ColumnEditor>();
        WhenRecursive(this, column, isSelectableOnly, result, isCurrentOnly: false);
        foreach (var item in result)
        {
            action(item);
        }

        if (result.Any()) MustRefresh = true;

        return this;
    }

    private void WhenRecursive(QueryNode node, string columnName, bool isSelectableOnly, List<ColumnEditor> result, bool isCurrentOnly)
    {
        var beforeCount = result.Count;

        // Search child nodes first
        if (!isCurrentOnly)
        {
            foreach (var datasourceNode in node.DatasourceNodeMap.Values)
            {
                foreach (var childQueryNode in datasourceNode.ChildQueryNodes)
                {
                    WhenRecursive(childQueryNode, columnName, isSelectableOnly, result, isCurrentOnly);
                }
            }

            if (result.Count != beforeCount)
            {
                return;
            }
        }

        // Search for column names used in the query
        if (node.SelectExpressionMap.ContainsKey(columnName))
        {
            result.Add(new(node.Query, node.SelectExpressionMap[columnName].Value));
            return;
        }

        if (isSelectableOnly)
        {
            return;
        }

        // Search for column names defined in the datasource
        var column = node.DatasourceNodeMap.Values.Where(ds => ds.Columns.ContainsKey(columnName)).FirstOrDefault();
        if (column != null)
        {
            var expr = new ColumnExpression(column.Name, column.Columns[columnName]);
            result.Add(new(node.Query, expr));
            return;
        }
    }

    private void WhenRecursive(QueryNode node, IList<string> columnNames, List<FromEditor> result, bool isCurrentOnly)
    {
        if (!isCurrentOnly)
        {
            // Search child nodes first
            foreach (var datasourceNode in node.DatasourceNodeMap.Values)
            {
                foreach (var childQueryNode in datasourceNode.ChildQueryNodes)
                {
                    WhenRecursive(childQueryNode, columnNames, result, isCurrentOnly);
                }
            }

            if (result.Any())
            {
                return;
            }
        }

        var values = new Dictionary<string, IValueExpression>();

        // Search for columns used in the query
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
                // Exit if any column is not found
                return;
            }
        }

        // Add to result if all columns are found
        result.Add(new(node, values));
    }

    private void WhenRecursive(QueryNode node, IList<string> columnNames, List<WhereEditor> result, bool isCurrentOnly)
    {
        if (!isCurrentOnly)
        {
            var startingCount = result.Count;

            // Search child nodes first
            foreach (var datasourceNode in node.DatasourceNodeMap.Values)
            {
                foreach (var childQueryNode in datasourceNode.ChildQueryNodes)
                {
                    WhenRecursive(childQueryNode, columnNames, result, isCurrentOnly);
                }
            }

            // Exit if any column is found
            if (startingCount != result.Count)
            {
                return;
            }
        }

        var values = new Dictionary<string, IValueExpression>();

        // Search for columns used in the query
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
                // Exit if any column is not found
                return;
            }
        }

        // Add to result if all columns are found
        result.Add(new(node.Query, values));
    }

    public string ToSql()
    {
        return Query.ToSql();
    }

    public IDictionary<string, object?> GetParameters()
    {
        return Query.GetParameters();
    }
    public QueryNode AddParameter(string name, object value)
    {
        Query.AddParameter(name, value);
        return this;
    }

    internal void Refresh()
    {
        var node = QueryAstParser.Parse(Query);
        Query = node.Query;
        SelectExpressionMap = node.SelectExpressionMap;
        DatasourceNodeMap = node.DatasourceNodeMap;

        MustRefresh = false;
    }

    public bool TryGetSelectQuery([NotNullWhen(true)] out ISelectQuery? selectQuery)
    {
        selectQuery = Query;
        return true;
    }

    public bool TryGetWhereClause([NotNullWhen(true)] out WhereClause? whereClause)
    {
        return Query.TryGetWhereClause(out whereClause);
    }

    public QueryNode UnionAll(IQuery right)
    {
        if (Query.TryGetSelectQuery(out var leftQuery) && right.TryGetSelectQuery(out var rightQuery))
        {
            Query = new UnionQuery("union all", leftQuery, rightQuery);
            MustRefresh = true;
            return this;
        }
        if (leftQuery is null)
        {
            throw new ArgumentException("The left query must be a select query.");
        }
        throw new ArgumentException("The right query must be a select query.", nameof(right));
    }

    public QueryNode Union(IQuery right)
    {
        if (Query.TryGetSelectQuery(out var leftQuery) && right.TryGetSelectQuery(out var rightQuery))
        {
            Query = new UnionQuery("union", leftQuery, rightQuery);
            MustRefresh = true;
            return this;
        }
        if (leftQuery is null)
        {
            throw new ArgumentException("The left query must be a select query.");
        }
        throw new ArgumentException("The right query must be a select query.", nameof(right));
    }

    public QueryNode Intersect(IQuery right)
    {
        if (Query.TryGetSelectQuery(out var leftQuery) && right.TryGetSelectQuery(out var rightQuery))
        {
            Query = new UnionQuery("intersect", leftQuery, rightQuery);
            MustRefresh = true;
            return this;
        }
        if (leftQuery is null)
        {
            throw new ArgumentException("The left query must be a select query.");
        }
        throw new ArgumentException("The right query must be a select query.", nameof(right));
    }

    public QueryNode Except(IQuery right)
    {
        if (Query.TryGetSelectQuery(out var leftQuery) && right.TryGetSelectQuery(out var rightQuery))
        {
            Query = new UnionQuery("except", leftQuery, rightQuery);
            MustRefresh = true;
            return this;
        }
        if (leftQuery is null)
        {
            throw new ArgumentException("The left query must be a select query.");
        }
        throw new ArgumentException("The right query must be a select query.", nameof(right));
    }

    public QueryNode Distinct()
    {
        if (Query.TryGetSelectClause(out var selectClause))
        {
            selectClause.DistinctClause = new DistinctClause();
            return this;
        }
        throw new InvalidOperationException("The query must have a select clause.");
    }

    public bool TryGetSelectClause([NotNullWhen(true)] out SelectClause? selectClause)
    {
        return Query.TryGetSelectClause(out selectClause);
    }

    internal void NormalizeSelectClause()
    {
        if (MustRefresh) Refresh();

        // The processing target is only the terminal SelectQuery
        if (Query is not SelectQuery sq)
        {
            throw new InvalidOperationException("ToSelectJson can only be used on a SelectQuery");
        }

        // columns
        var columns = SelectExpressionMap
            .Where(x => x.Value.Value is ColumnExpression ce);

        foreach (var column in columns)
        {
            var ce = (ColumnExpression)column.Value.Value;
            column.Value.Alias = $"{ce.NamespaceFullName}__{column.Value.Alias}";
        }
        MustRefresh = true;
    }

    public QueryNode ToJsonQuery()
    {
        return ToJsonQuery(false, x => x, x => x);
    }

    public QueryNode ToJsonQuery(bool columnNormalization, Func<PostgresJsonEditor, PostgresJsonEditor> action)
    {
        return ToJsonQuery(columnNormalization, x => x, action);
    }

    public QueryNode ToJsonQuery(bool columnNormalization, Func<string, string> propertyBuilder, Func<PostgresJsonEditor, PostgresJsonEditor> action)
    {
        if (MustRefresh) Refresh();
        // The processing target is only the terminal SelectQuery
        if (Query is not SelectQuery sq)
        {
            throw new InvalidOperationException("ToJson can only be used on a SelectQuery");
        }

        if (columnNormalization)
        {
            NormalizeSelectClause();
        }

        ToCteQuery("__json", overrideNode: true);

        var editor = action(new PostgresJsonEditor(this, propertyBuilder: propertyBuilder));

        // NOTE:
        // The column alias of the select query follows the naming convention "datasource__property".
        // Columns that are not normalized are changed from "datasource__property" to "property" as properties belonging to the root.
        // Also, property names are escaped with double quotes to distinguish between uppercase and lowercase letters.
        if (editor.Query.TryGetSelectClause(out var selectClause))
        {
            foreach (var item in selectClause.Expressions)
            {
                var alias = item.Alias;
                alias = alias.StartsWith("\"") ? alias : $"\"{alias}\"";
                if (alias.Contains("__"))
                {
                    alias = "\"" + alias.Substring(alias.LastIndexOf("__") + 2);
                }
                item.Alias = alias;
            }
        }

        var text = "row_to_json(d)";
        var newQuery = new SelectQuery(
            new SelectClause(SelectExpressionParser.Parse(text)),
            new FromClause(new DatasourceExpression(new SubQuerySource(editor.Query), "d"))
            );
        newQuery.LimitClause = new LimitClause(new LiteralExpression("1"));

        Query = newQuery;
        MustRefresh = true;
        return this;
    }

}
