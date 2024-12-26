using Carbunqlex.DatasourceExpressions;
using Carbunqlex.ValueExpressions;

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
    IEnumerable<IDatasource> GetDatasources();
}
