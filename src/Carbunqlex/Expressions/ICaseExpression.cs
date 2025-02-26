using Carbunqlex.Clauses;

namespace Carbunqlex.Expressions;

public interface ICaseExpression : IValueExpression
{
    List<WhenClause> WhenClauses { get; }
    IValueExpression? ElseValue { get; }
}
