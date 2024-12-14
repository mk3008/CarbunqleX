namespace Carbunqlex.Clauses;

public interface IOrderByClause : ISqlComponent
{
    bool MightHaveCommonTableClauses { get; }
}
