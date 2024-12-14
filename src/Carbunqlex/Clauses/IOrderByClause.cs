namespace Carbunqlex.Clauses;

public interface IOrderByClause : ISqlComponent
{
    bool MightHaveQueries { get; }
}
