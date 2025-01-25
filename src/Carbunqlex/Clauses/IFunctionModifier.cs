namespace Carbunqlex.Clauses;

public interface IFunctionModifier : ISqlComponent
{
    bool MightHaveQueries { get; }
}
