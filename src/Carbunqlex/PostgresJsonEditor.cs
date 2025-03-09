﻿using Carbunqlex.Expressions;
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
            .Select(col => $"'{JsonKeyFormatter?.Invoke(col.Value.Alias) ?? col.Value.Alias}', {col.Value.Value.ToSqlWithoutCte()}")
            .ToList();

        var exp = $"json_build_object({string.Join(", ", columnStrings)}) AS {objectName}";

        sq.SelectClause.Expressions.Add(SelectExpressionParser.Parse(exp));

        MustRefresh = true;
        return this;
    }

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
