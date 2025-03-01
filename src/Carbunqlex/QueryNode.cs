using Carbunqlex.Clauses;
using Carbunqlex.Expressions;
using Carbunqlex.Lexing;
using Carbunqlex.Parsing.Expressions;
using Carbunqlex.Parsing.QuerySources;
using Carbunqlex.QuerySources;
using System.Collections.ObjectModel;
using System.Text;

namespace Carbunqlex;

public class QueryNode : ISqlComponent, IQuery
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

    //public QueryNode Where(string columnName, Action<WhereEditorOld> action)
    //{
    //    if (MustRefresh) Refresh();

    //    var result = GetColumnEditors(columnName, isSelectableOnly: false, isCurrentOnly: false);

    //    result = result.GroupBy(x => x.Value).Select(g => g.First()).ToList();

    //    foreach (var columnModifier in result)
    //    {
    //        var editor = new WhereEditorOld(columnModifier);
    //        action(editor);
    //    }

    //    if (result.Any()) MustRefresh = true;

    //    return this;
    //}

    public QueryNode From(IEnumerable<string> columnNames, Action<FromEditor> action)
    {
        if (MustRefresh) Refresh();

        var result = GetFromEditors(columnNames);

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

    public QueryNode NormalizeSelectClause()
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
            column.Value.Alias = $"{ce.NamespaceFullName}_{column.Value.Alias}";
        }
        MustRefresh = true;
        return this;
    }

    public QueryNode AddJsonColumn(string datasourceName, string objectName, bool removeNotStructColumn = true, Func<string, string>? propertyBuilder = null)
    {
        if (MustRefresh) Refresh();

        // The processing target is only the terminal SelectQuery
        if (Query is not SelectQuery sq)
        {
            throw new InvalidOperationException("ToSelectJson can only be used on a SelectQuery");
        }

        // datasource
        var ds = sq.GetDatasources().Where(x => x.Alias.Equals(datasourceName, StringComparison.InvariantCultureIgnoreCase)).First();

        // columns
        var columns = SelectExpressionMap
            .Where(x => x.Value.Value is ColumnExpression ce && ce.NamespaceFullName.Equals(datasourceName, StringComparison.InvariantCultureIgnoreCase)).ToList();

        if (columns.Count() == 0)
        {
            throw new InvalidOperationException($"No columns found for datasource '{datasourceName}'");
        }

        if (removeNotStructColumn)
        {
            foreach (var column in columns)
            {
                sq.SelectClause.Expressions.Remove(column.Value);
            }
        }

        var columnStrings = columns
            .Select(col => $"'{propertyBuilder?.Invoke(col.Value.Alias) ?? col.Value.Alias}', {col.Value.Value.ToSqlWithoutCte()}")
            .ToList();

        var exp = $"json_build_object({string.Join(", ", columnStrings)}) AS {objectName}";

        sq.SelectClause.Expressions.Add(SelectExpressionParser.Parse(exp));

        MustRefresh = true;

        return this;
    }

    public QueryNode Serialize(string datasource, string objectName = "", IEnumerable<string>? include = null, bool removePropertyColumn = true, Func<string, string>? propertyBuilder = null, bool removePrefix = true)
    {
        if (MustRefresh) Refresh();

        if (string.IsNullOrEmpty(objectName))
        {
            objectName = datasource;
        }

        // The processing target is only the terminal SelectQuery
        if (Query is not SelectQuery sq)
        {
            throw new InvalidOperationException("ToSelectJson can only be used on a SelectQuery");
        }

        // columns
        var propertyColumns = SelectExpressionMap
            .Where(x => x.Key.StartsWith(datasource + '_', StringComparison.InvariantCultureIgnoreCase)).ToList();

        if (propertyColumns.Count() == 0)
        {
            throw new InvalidOperationException($"No columns found for prefix '{datasource}'");
        }

        if (removePrefix)
        {
            foreach (var column in propertyColumns)
            {
                column.Value.Alias = column.Key.Substring(datasource.Length + 1);
            }
        }

        if (include != null)
        {
            foreach (var parentName in include)
            {
                var parent = SelectExpressionMap.Where(x => x.Key.Equals(parentName, StringComparison.InvariantCultureIgnoreCase)).First();
                propertyColumns.Add(parent);
            }
        }

        if (removePropertyColumn)
        {
            foreach (var column in propertyColumns)
            {
                sq.SelectClause.Expressions.Remove(column.Value);
            }
        }

        var columnStrings = propertyColumns
            .Select(col => $"'{propertyBuilder?.Invoke(col.Value.Alias) ?? col.Value.Alias}', {col.Value.Value.ToSqlWithoutCte()}")
            .ToList();

        var exp = $"json_build_object({string.Join(", ", columnStrings)}) as {objectName}";

        sq.SelectClause.Expressions.Add(SelectExpressionParser.Parse(exp));

        MustRefresh = true;

        return this;
    }

    public QueryNode ToJson()
    {
        if (MustRefresh) Refresh();

        // The processing target is only the terminal SelectQuery
        if (Query is not SelectQuery sq)
        {
            throw new InvalidOperationException("ToJson can only be used on a SelectQuery");
        }

        var text = "row_to_json(d)";
        var newQuery = new SelectQuery(
            new SelectClause(SelectExpressionParser.Parse(text)),
            new FromClause(new DatasourceExpression(new SubQuerySource(sq), "d"))
            );
        newQuery.LimitClause = new LimitClause(new LiteralExpression("1"));

        Query = newQuery;
        MustRefresh = true;
        return this;
    }

    public QueryNode ToJsonArray()
    {
        if (MustRefresh) Refresh();

        // The processing target is only the terminal SelectQuery
        if (Query is not SelectQuery sq)
        {
            throw new InvalidOperationException("ToJson can only be used on a SelectQuery");
        }

        var text = "json_agg(row_to_json(d))";
        var newQuery = new SelectQuery(
            new SelectClause(SelectExpressionParser.Parse(text)),
            new FromClause(new DatasourceExpression(new SubQuerySource(sq), "d"))
            );

        Query = newQuery;
        MustRefresh = true;
        return this;
    }

    private List<ColumnEditor> GetColumnEditors(string columnName, bool isSelectableOnly, bool isCurrentOnly)
    {
        var result = new List<ColumnEditor>();
        WhenRecursive(this, columnName.ToLowerInvariant(), isSelectableOnly, result, isCurrentOnly);
        return result;
    }

    private List<FromEditor> GetFromEditors(IEnumerable<string> columnNames)
    {
        var result = new List<FromEditor>();
        WhenRecursive(this, columnNames.Select(c => c.ToLowerInvariant()).ToList(), result, isCurrentOnly: true);
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
        result.Add(new(node.Query, values));
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

    public IEnumerable<Token> GenerateTokens()
    {
        return Query.GenerateTokens();
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

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        return Query.GenerateTokensWithoutCte();
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
        var node = QueryAstParser.Parse(Query);
        Query = node.Query;
        SelectExpressionMap = node.SelectExpressionMap;
        DatasourceNodeMap = node.DatasourceNodeMap;

        MustRefresh = false;
    }
}
