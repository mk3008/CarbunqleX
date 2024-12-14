namespace Carbunqlex.Clauses;

public interface IWindowFrameBoundaryExpression : ISqlComponent
{
    string BoundaryKeyword { get; }
    bool MightHaveCommonTableClauses { get; }
}
