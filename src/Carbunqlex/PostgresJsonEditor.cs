using Carbunqlex.Expressions;
using Carbunqlex.Parsing.Expressions;
using System.Collections.ObjectModel;

namespace Carbunqlex;

/// <summary>
/// Utility class using Postgres-specific JSON functions
/// </summary>
/// <param name="node"></param>
public class PostgresJsonEditor(QueryNode node, string? owner = null, Func<string, string>? jsonKeyFormatter = null)
{
    private QueryNode Node { get; } = node;
    private string? Owner { get; } = owner;

    private bool MustRefresh
    {
        get => Node.MustRefresh;
        set => Node.MustRefresh = value;
    }

    Func<string, string>? JsonKeyFormatter { get; set; } = jsonKeyFormatter;

    private void Refresh() => Node.Refresh();

    internal ISelectQuery Query => Node.Query;

    private ReadOnlyDictionary<string, SelectExpression> SelectExpressionMap => Node.SelectExpressionMap;

    internal PostgresJsonEditor AddJsonColumn(string datasourceName, string objectName, bool removeNotStructColumn = true)
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
            .Select(col => $"'{JsonKeyFormatter?.Invoke(col.Value.Alias) ?? col.Value.Alias}', {col.Value.Value.ToSqlWithoutCte()}")
            .ToList();

        var exp = $"json_build_object({string.Join(", ", columnStrings)}) AS {objectName}";

        sq.SelectClause.Expressions.Add(SelectExpressionParser.Parse(exp));

        MustRefresh = true;
        return this;
    }

    /// <summary>
    /// Converts columns from a specified datasource into a JSON object using PostgreSQL's json_build_object function.
    /// </summary>
    /// <param name="datasource">The alias of the datasource whose columns will be serialized.</param>
    /// <param name="jsonKey">The name to use for the JSON object in the result. If empty, uses the datasource name.</param>
    /// <param name="parent">Optional function to define nested JSON structure for related entities.</param>
    /// <param name="isFlat">When true, doesn't remove the original columns from the result. Default is false.</param>
    /// <returns>A new PostgresJsonEditor instance with the modified query structure.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no columns are found with the specified datasource prefix or when the query is not a SelectQuery.
    /// </exception>
    /// <remarks>
    /// This method looks for columns that follow the naming pattern "{datasource}__columnName" and transforms them into a JSON object.
    /// The prefix is removed from each column name during serialization, and property names in the JSON can be formatted using the jsonKeyFormatter.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create a JSON object with user properties
    /// editor.Serialize(datasource: "user", jsonKey: "Author");
    /// 
    /// // Create a nested JSON structure
    /// editor.Serialize(datasource: "blog", jsonKey: "Blog", parent: e => 
    ///     e.Serialize(datasource: "category", jsonKey: "Category")
    /// );
    /// </code>
    /// </example>
    public PostgresJsonEditor Serialize(
        string datasource,
        string jsonKey = "",
        Func<PostgresJsonEditor, PostgresJsonEditor>? parent = null,
        bool isFlat = false)
    {
        if (MustRefresh) Refresh();

        if (string.IsNullOrEmpty(jsonKey))
        {
            jsonKey = datasource;
        }

        var editor = parent != null ? parent.Invoke(new PostgresJsonEditor(Node, owner: datasource, jsonKeyFormatter: JsonKeyFormatter)) : this;

        // columns
        var propertyColumns = editor.SelectExpressionMap
            .Where(x => x.Value.Alias.StartsWith(datasource + "__", StringComparison.InvariantCultureIgnoreCase)).ToList();

        if (propertyColumns.Count() == 0)
        {
            throw new InvalidOperationException($"No columns found for prefix '{datasource}'");
        }

        // Remove prefix from column alias
        foreach (var column in propertyColumns)
        {
            column.Value.Alias = column.Value.Alias.Substring(datasource.Length + 2);
        }

        // The processing target is only the terminal SelectQuery
        if (editor.Query is not SelectQuery sq)
        {
            throw new InvalidOperationException("ToSelectJson can only be used on a SelectQuery");
        }

        if (!isFlat)
        {
            // Remove serialize component columns
            foreach (var column in propertyColumns)
            {
                sq.SelectClause.Expressions.Remove(column.Value);
            }

            // add serialize column
            var columnStrings = propertyColumns
                .Select(col => $"'{JsonKeyFormatter?.Invoke(col.Value.Alias) ?? col.Value.Alias}', {col.Value.Value.ToSqlWithoutCte()}")
                .ToList();

            var alias = string.IsNullOrEmpty(Owner) ? jsonKey : Owner + "__" + jsonKey;

            var exp = $"json_build_object({string.Join(", ", columnStrings)}) as {alias}";

            sq.SelectClause.Expressions.Add(SelectExpressionParser.Parse(exp));
        }

        var newNode = QueryAstParser.Parse(sq);
        return new PostgresJsonEditor(newNode, owner: Owner, jsonKeyFormatter: JsonKeyFormatter);
    }

    /// <summary>
    /// Converts columns from a specified datasource into a JSON array of objects using PostgreSQL's json_agg function.
    /// </summary>
    /// <param name="datasource">The alias of the datasource whose columns will be serialized into an array of objects.</param>
    /// <param name="jsonKey">The name to use for the JSON array in the result. If empty, uses the datasource name.</param>
    /// <param name="parent">Optional function to define nested JSON structure for each object in the array.</param>
    /// <returns>A new PostgresJsonEditor instance with the modified query structure.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no columns are found with the specified datasource prefix or when the query is not a SelectQuery.
    /// </exception>
    /// <remarks>
    /// This method looks for columns that follow the naming pattern "{datasource}__columnName" and transforms them into 
    /// an array of JSON objects using PostgreSQL's json_agg function. The prefix is removed from each column name during serialization.
    /// 
    /// The method automatically adds the necessary GROUP BY clauses for non-serialized columns and creates a Common Table Expression (CTE)
    /// named "__json_{jsonKey}" to hold the aggregated array result.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create an array of comment objects
    /// editor.SerializeArray(datasource: "comment", jsonKey: "Comments");
    /// 
    /// // Create an array with nested objects
    /// editor.SerializeArray(datasource: "post", jsonKey: "Posts", parent: e => 
    ///     e.Serialize(datasource: "author", jsonKey: "Author")
    /// );
    /// </code>
    /// </example>
    public PostgresJsonEditor SerializeArray(
        string datasource,
        string jsonKey = "",
        Func<PostgresJsonEditor, PostgresJsonEditor>? parent = null)
    {
        if (MustRefresh) Refresh();

        if (string.IsNullOrEmpty(jsonKey))
        {
            jsonKey = datasource;
        }

        var editor = parent != null ? parent.Invoke(new PostgresJsonEditor(Node, owner: datasource, jsonKeyFormatter: JsonKeyFormatter)) : this;

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
            .Select(col => $"'{JsonKeyFormatter?.Invoke(col.Value.Alias) ?? col.Value.Alias}', {col.Value.Value.ToSqlWithoutCte()}")
            .ToList();

        var alias = string.IsNullOrEmpty(Owner) ? jsonKey : $"{Owner}__{jsonKey}";
        var exp = $"json_agg(json_build_object({string.Join(", ", columnStrings)})) as {alias}";

        sq.SelectClause.Expressions.Add(SelectExpressionParser.Parse(exp));

        // convert to CTE
        var newNode = QueryAstParser.Parse(sq);
        newNode = newNode.ToCteQuery($"__json_{jsonKey}");

        return new PostgresJsonEditor(newNode, owner: Owner, jsonKeyFormatter: JsonKeyFormatter);
    }
}
