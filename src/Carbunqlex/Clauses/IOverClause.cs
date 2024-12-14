namespace Carbunqlex.Clauses;

public interface IOverClause : ISqlComponent
{
    bool MightHaveCommonTableClauses { get; }
}
