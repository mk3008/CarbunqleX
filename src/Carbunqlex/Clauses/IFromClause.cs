using Carbunqlex.Expressions;
using Carbunqlex.QuerySources;

namespace Carbunqlex.Clauses;

public interface IFromClause : ISqlComponent
{
    /// <summary>
    /// Retrieves the selectable columns.
    /// </summary>
    /// <returns></returns>
    IEnumerable<ColumnExpression> GetSelectableColumnExpressions();

    /// <summary>
    /// Retrieves the datasources.
    /// </summary>
    IEnumerable<DatasourceExpression> GetDatasources();

    /// <summary>
    /// Retrieves the column expressions.
    /// </summary>
    /// <returns></returns>
    IEnumerable<ColumnExpression> ExtractColumnExpressions();

    void AddJoin(JoinClause joinClause);
}
