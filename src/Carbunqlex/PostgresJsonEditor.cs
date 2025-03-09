using Carbunqlex.Expressions;
using Carbunqlex.Parsing.Expressions;
using System.Collections.ObjectModel;

namespace Carbunqlex;

/// <summary>
/// Utility class using Postgres-specific JSON functions
/// </summary>
/// <param name="node"></param>
public class PostgresJsonEditor(QueryNode node, string? owner = null, Func<string, string>? propertyBuilder = null)
{
    private QueryNode Node { get; } = node;
    private string? Owner { get; } = owner;

    private bool MustRefresh
    {
        get => Node.MustRefresh;
        set => Node.MustRefresh = value;
    }

    Func<string, string>? PropertyBuilder { get; set; } = propertyBuilder;

    private void Refresh() => Node.Refresh();

    internal ISelectQuery Query => Node.Query;

    private ReadOnlyDictionary<string, SelectExpression> SelectExpressionMap => Node.SelectExpressionMap;

    public PostgresJsonEditor AddJsonColumn(string datasourceName, string objectName, bool removeNotStructColumn = true)
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
            .Select(col => $"'{PropertyBuilder?.Invoke(col.Value.Alias) ?? col.Value.Alias}', {col.Value.Value.ToSqlWithoutCte()}")
            .ToList();

        var exp = $"json_build_object({string.Join(", ", columnStrings)}) AS {objectName}";

        sq.SelectClause.Expressions.Add(SelectExpressionParser.Parse(exp));

        MustRefresh = true;
        return this;
    }

    public PostgresJsonEditor Serialize(
        string datasource,
        string objectName = "",
        IEnumerable<string>? include = null,
        Func<PostgresJsonEditor, PostgresJsonEditor>? upperNode = null,
        bool removePropertyColumn = true,
        bool removePrefix = true)
    {
        if (MustRefresh) Refresh();

        if (string.IsNullOrEmpty(objectName))
        {
            objectName = datasource;
        }

        var editor = upperNode != null ? upperNode.Invoke(new PostgresJsonEditor(Node, owner: datasource, propertyBuilder: PropertyBuilder)) : this;

        // columns
        var propertyColumns = editor.SelectExpressionMap
            .Where(x => x.Value.Alias.StartsWith(datasource + "__", StringComparison.InvariantCultureIgnoreCase)).ToList();

        if (propertyColumns.Count() == 0)
        {
            throw new InvalidOperationException($"No columns found for prefix '{datasource}'");
        }

        if (removePrefix)
        {
            foreach (var column in propertyColumns)
            {
                column.Value.Alias = column.Value.Alias.Substring(datasource.Length + 2);
            }
        }

        if (include != null)
        {
            foreach (var parentName in include)
            {
                var parent = editor.SelectExpressionMap.Where(x => x.Value.Alias.Equals(parentName, StringComparison.InvariantCultureIgnoreCase)).First();
                propertyColumns.Add(parent);
            }
        }

        // The processing target is only the terminal SelectQuery
        if (editor.Query is not SelectQuery sq)
        {
            throw new InvalidOperationException("ToSelectJson can only be used on a SelectQuery");
        }

        if (removePropertyColumn)
        {
            foreach (var column in propertyColumns)
            {
                sq.SelectClause.Expressions.Remove(column.Value);
            }
        }

        var columnStrings = propertyColumns
            .Select(col => $"'{PropertyBuilder?.Invoke(col.Value.Alias) ?? col.Value.Alias}', {col.Value.Value.ToSqlWithoutCte()}")
            .ToList();

        var alias = string.IsNullOrEmpty(Owner) ? objectName : Owner + "__" + objectName;

        var exp = $"json_build_object({string.Join(", ", columnStrings)}) as {alias}";

        sq.SelectClause.Expressions.Add(SelectExpressionParser.Parse(exp));

        var newNode = QueryAstParser.Parse(sq);

        return new PostgresJsonEditor(newNode, owner: Owner, propertyBuilder: PropertyBuilder);
    }

    public PostgresJsonEditor ArraySerialize(
        string datasource,
        string objectName = "",
        IEnumerable<string>? include = null,
        Func<PostgresJsonEditor, PostgresJsonEditor>? upperNode = null)
    {
        if (MustRefresh) Refresh();

        if (string.IsNullOrEmpty(objectName))
        {
            objectName = datasource;
        }

        var editor = upperNode != null ? upperNode.Invoke(new PostgresJsonEditor(Node, owner: datasource, propertyBuilder: PropertyBuilder)) : this;

        // columns
        var serializeTargetColumns = editor.SelectExpressionMap
            .Where(x => x.Value.Alias.StartsWith(datasource + "__", StringComparison.InvariantCultureIgnoreCase)).ToList();

        if (serializeTargetColumns.Count() == 0)
        {
            throw new InvalidOperationException($"No columns found for prefix '{datasource}'");
        }

        // Remove prefix from column alias
        foreach (var column in serializeTargetColumns)
        {
            column.Value.Alias = column.Value.Alias.Substring(datasource.Length + 2);
        }

        // The processing target is only the terminal SelectQuery
        if (editor.Query is not SelectQuery sq)
        {
            throw new InvalidOperationException("ToSelectJson can only be used on a SelectQuery");
        }

        // grouping column from the original query
        var groupColumns = sq.GetSelectExpressions()
            .Where(x => !serializeTargetColumns.Any(y => y.Value == x))
            .SelectMany(x => x.Value.ExtractColumnExpressions())
            .ToList();

        // remove serialize component columns
        foreach (var column in serializeTargetColumns)
        {
            sq.SelectClause.Expressions.Remove(column.Value);
        }

        foreach (var column in groupColumns)
        {
            sq.GroupByClause.Add(column);
        }

        // add serialize column
        var columnStrings = serializeTargetColumns
            .Select(col => $"'{PropertyBuilder?.Invoke(col.Value.Alias) ?? col.Value.Alias}', {col.Value.Value.ToSqlWithoutCte()}")
            .ToList();

        var alias = string.IsNullOrEmpty(Owner) ? objectName : $"{Owner}__{objectName}";
        var exp = $"json_agg(json_build_object({string.Join(", ", columnStrings)})) as {alias}";

        sq.SelectClause.Expressions.Add(SelectExpressionParser.Parse(exp));

        // convert to CTE
        var newNode = QueryAstParser.Parse(sq);
        newNode = newNode.ToCteQuery($"__json_{objectName}");

        return new PostgresJsonEditor(newNode, owner: Owner, propertyBuilder: PropertyBuilder);
    }
}
