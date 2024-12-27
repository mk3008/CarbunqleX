using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Clauses;

public interface IWhereClause : ISqlComponent
{
    /// <summary>
    /// Retrieves the column expressions.
    /// </summary>
    /// <returns></returns>
    IEnumerable<ColumnExpression> ExtractColumnExpressions();
}
