namespace Carbunqlex.Clauses;

public interface IColumnAliasClause : ISqlComponent
{
    IEnumerable<string> GetColumnNames();
}
