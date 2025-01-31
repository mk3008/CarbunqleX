using Carbunqlex.DatasourceExpressions;
using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Clauses;

public class EmptyFromClause : IFromClause
{
    // Singleton instance
    public static readonly EmptyFromClause Instance = new EmptyFromClause();


    // Private constructor to prevent instantiation
    private EmptyFromClause() { }

    public string ToSqlWithoutCte()
    {
        return string.Empty;
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        return Enumerable.Empty<Token>();
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return Enumerable.Empty<ISelectQuery>();
    }

    public IEnumerable<ColumnExpression> GetSelectableColumnExpressions()
    {
        return Enumerable.Empty<ColumnExpression>();
    }

    public IEnumerable<DatasourceExpression> GetDatasources()
    {
        return Enumerable.Empty<DatasourceExpression>();
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        return Enumerable.Empty<ColumnExpression>();
    }

    public void AddJoin(JoinClause joinClause)
    {
        throw new NotSupportedException("Cannot add a join to an empty from clause.");
    }
}
