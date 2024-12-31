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

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        return Enumerable.Empty<Lexeme>();
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return Enumerable.Empty<ISelectQuery>();
    }

    public IEnumerable<ColumnExpression> GetSelectableColumnExpressions()
    {
        return Enumerable.Empty<ColumnExpression>();
    }

    public IEnumerable<IDatasource> GetDatasources()
    {
        return Enumerable.Empty<IDatasource>();
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        return Enumerable.Empty<ColumnExpression>();
    }
}
