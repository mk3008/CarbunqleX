namespace Carbunqlex.Clauses;

public interface IWindowFrame : ISqlComponent
{
    bool MightHaveCommonTableClauses { get; }
}
