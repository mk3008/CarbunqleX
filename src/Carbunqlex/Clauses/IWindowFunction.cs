namespace Carbunqlex.Clauses;

public interface IWindowFunction : ISqlComponent
{
    IPartitionByClause PartitionBy { get; }
    IOrderByClause OrderBy { get; }
    IWindowFrame WindowFrame { get; }

    bool MightHaveCommonTableClauses { get; }
}
