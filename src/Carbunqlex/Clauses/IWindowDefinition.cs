namespace Carbunqlex.Clauses;

public interface IWindowDefinition : ISqlComponent
{
    bool MightHaveCommonTableClauses { get; }
}
