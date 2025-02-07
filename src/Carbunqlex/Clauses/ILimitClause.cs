namespace Carbunqlex.Clauses;

public interface ILimitClause : ISqlComponent
{
    public bool IsLimit { get; }
}
