namespace Carbunqlex.Clauses;

public interface IPartitionByClause : ISqlComponent
{
    bool MightHaveQueries { get; }
}
