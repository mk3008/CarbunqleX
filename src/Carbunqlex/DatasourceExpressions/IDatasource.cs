using Carbunqlex.ValueExpressions;

namespace Carbunqlex.DatasourceExpressions;

public interface IDatasource : ISqlComponent
{
    /// <summary>
    /// The alias of the datasource.
    /// </summary>
    string Alias { get; set; }

    /// <summary>
    /// Retrieves the selectable columns.
    /// </summary>
    /// <returns></returns>
    IEnumerable<ColumnExpression> GetSelectableColumns();
}
