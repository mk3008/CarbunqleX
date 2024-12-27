using Carbunqlex.Clauses;

namespace Carbunqlex;

public class QueryNodeFactory
{
    public static QueryNode Create(IQuery query)
    {
        var ctes = query.GetCommonTableClauses().ToList();
        return Create(ctes, query);
    }

    public static QueryNode Create(IList<CommonTableClause> ctes, IQuery query)
    {
        var datasourceNodes = new List<DatasourceNode>();

        foreach (var datasource in query.GetDatasources())
        {
            var alias = datasource.Alias;
            var childQueryNodes = new List<QueryNode>();

            if (datasource.TryGetSubQuery(out var subQuery))
            {
                // If the datasource is a subquery, recursively generate query nodes
                childQueryNodes.Add(Create(subQuery));

                var datasourceColumns = subQuery.GetSelectExpressions().Select(static expr => expr.Alias);
                var datasourceNode = new DatasourceNode(datasource, DatasourceType.SubQuery, datasourceColumns, childQueryNodes);
                datasourceNodes.Add(datasourceNode);
            }
            else if (datasource.TryGetUnionQuerySource(out var unionQuerySource))
            {
                // If the datasource is a union query, recursively generate query nodes
                childQueryNodes.Add(Create(unionQuerySource.Query));

                var datasourceColumns = datasource.GetSelectableColumns();
                var datasourceNode = new DatasourceNode(datasource, DatasourceType.UnionSubQuery, datasourceColumns, childQueryNodes);

                datasourceNodes.Add(datasourceNode);
            }
            else if (datasource.TryGetTableName(out var table) && ctes.Where(cte => cte.Alias == table).Any())
            {
                // If the datasource is a CTE, recursively generate query nodes
                var cte = ctes.Where(cte => cte.Alias == table).First();
                childQueryNodes.Add(Create(ctes, cte.Query));

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
