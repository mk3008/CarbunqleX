using Carbunqlex.Clauses;
using Carbunqlex.Parsing;

namespace Carbunqlex;

/// <summary>
/// Parses a query AST from a SQL string.
/// </summary>
public class QueryAstParser
{
    /// <summary>
    /// Parses a query AST from a SQL string.
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    public static QueryNode Parse(string sql)
    {
        var query = SelectQueryParser.Parse(sql);
        return Parse(query);
    }

    /// <summary>
    /// Parses a query AST from a select query.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public static QueryNode Parse(ISelectQuery query)
    {
        var ctes = query.GetCommonTableClauses().ToList();
        return CreateCore(ctes, query);
    }

    private static QueryNode CreateCore(IList<CommonTableClause> ctes, ISelectQuery query)
    {
        var datasourceNodes = new List<DatasourceNode>();

        foreach (var datasource in query.GetDatasources())
        {
            var alias = datasource.Alias;
            var childQueryNodes = new List<QueryNode>();

            if (datasource.TryGetSubQuery(out var subQuery))
            {
                // If the datasource is a subquery, recursively generate query nodes
                var child = CreateCore(ctes, subQuery);
                childQueryNodes.Add(child);

                var datasourceColumns = subQuery.GetSelectExpressions().Select(static expr => expr.Alias).Where(static x => x != "*").ToList();

                if (datasourceColumns.Any())
                {
                    var datasourceNode = new DatasourceNode(datasource, DatasourceType.SubQuery, datasourceColumns, childQueryNodes);
                    datasourceNodes.Add(datasourceNode);
                }
                else
                {
                    // If the datasource cannot retrieve columns due to a wildcard, use the columns from the child datasource
                    var selectedColumns = child.DatasourceNodeMap.SelectMany(static node => node.Value.Columns.Select(static column => column.Value)).Distinct();
                    var datasourceNode = new DatasourceNode(datasource, DatasourceType.SubQuery, selectedColumns, childQueryNodes);
                    datasourceNodes.Add(datasourceNode);
                }
            }
            else if (datasource.TryGetUnionQuerySource(out var unionQuerySource))
            {
                // If the datasource is a union query, recursively generate query nodes
                var child = CreateCore(ctes, unionQuerySource.Query);
                childQueryNodes.Add(child);

                var datasourceColumns = datasource.GetSelectableColumns();
                var datasourceNode = new DatasourceNode(datasource, DatasourceType.UnionSubQuery, datasourceColumns, childQueryNodes);

                datasourceNodes.Add(datasourceNode);
            }
            else if (datasource.TryGetTableName(out var table) && ctes.Where(cte => cte.Alias == table).Any())
            {
                // If the datasource is a CTE, recursively generate query nodes
                var cte = ctes.Where(cte => cte.Alias == table).First();
                childQueryNodes.Add(CreateCore(ctes, cte.Query));

                var columnAliases = cte.ColumnAliasClause?.ColumnAliases;
                if (columnAliases != null)
                {
                    var datasourceNode = new DatasourceNode(datasource, DatasourceType.CommonTableExtension, columnAliases, childQueryNodes);
                    datasourceNodes.Add(datasourceNode);
                }
                else
                {
                    var datasourceColumns = cte.Query.GetSelectExpressions().Select(static expr => expr.Alias);
                    var datasourceNode = new DatasourceNode(datasource, DatasourceType.CommonTableExtension, datasourceColumns, childQueryNodes);
                    datasourceNodes.Add(datasourceNode);
                }
            }
            else
            {
                var datasourceColumns = datasource.GetSelectableColumns().ToList();

                if (datasourceColumns.Any())
                {
                    // If the datasource has selectable columns, use them
                    var datasourceNode = new DatasourceNode(datasource, DatasourceType.Table, datasource.GetSelectableColumns(), childQueryNodes);
                    datasourceNodes.Add(datasourceNode);
                }
                else
                {
                    // If the datasource has no selectable columns, use the columns from the query
                    var queryColumns = query.ExtractColumnExpressions();
                    var columnComponents = queryColumns.Where(x => string.IsNullOrEmpty(x.NamespaceFullName) || x.NamespaceFullName.ToLowerInvariant() == datasource.Alias.ToLowerInvariant()).Select(static x => x.ColumnName).Distinct();

                    var datasourceNode = new DatasourceNode(datasource, DatasourceType.Table, columnComponents, childQueryNodes);
                    datasourceNodes.Add(datasourceNode);
                }
            }
        }
        return new QueryNode(query, datasourceNodes);
    }
}
