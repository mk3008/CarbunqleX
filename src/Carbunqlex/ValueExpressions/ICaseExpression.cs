using Carbunqlex.Clauses;

namespace Carbunqlex.ValueExpressions;

public interface ICaseExpression : IValueExpression
{
    List<WhenClause> WhenClauses { get; }
    IValueExpression? ElseValue { get; }
}
