using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Clauses;

public class DistinctOnClause : IDistinctClause
{
    public bool IsDistinct { get; } = true;
    public List<IValueExpression> DistinctOnColumns { get; }

    public DistinctOnClause(params IValueExpression[] distinctOnColumns)
    {
        DistinctOnColumns = distinctOnColumns.ToList();
    }

    public string ToSqlWithoutCte()
    {
        return $"distinct on ({string.Join(", ", DistinctOnColumns.Select(c => c.ToSqlWithoutCte()))})";
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.Keyword, "distinct");
        yield return new Token(TokenType.Keyword, "on");
        yield return new Token(TokenType.OpenParen, "(");

        foreach (var lexeme in DistinctOnColumns.SelectMany(c => c.GenerateTokensWithoutCte()))
        {
            yield return lexeme;
        }

        yield return new Token(TokenType.CloseParen, ")");
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery>();

        foreach (var column in DistinctOnColumns)
        {
            if (column.MightHaveQueries)
            {
                queries.AddRange(column.GetQueries());
            }
        }

        return queries;
    }
}
