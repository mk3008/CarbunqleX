namespace Carbunqlex.Clauses;

public interface IPartitionByClause : ISqlComponent
{
    bool MightHaveCommonTableClauses { get; }
}
