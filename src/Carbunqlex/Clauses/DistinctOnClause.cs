using Carbunqlex.Expressions;
using Carbunqlex.Lexing;

namespace Carbunqlex.Clauses;

public class DistinctOnClause : IDistinctClause
{
    public bool IsDistinct { get; } = true;

    public List<IValueExpression> DistinctOnColumns { get; }

    public DistinctOnClause(params IValueExpression[] distinctOnColumns)
    {
        DistinctOnColumns = distinctOnColumns.ToList();
    }

    public DistinctOnClause(List<IValueExpression> distinctOnColumns)
    {
        DistinctOnColumns = distinctOnColumns;
    }

    public string ToSqlWithoutCte()
    {
        return $"distinct on ({string.Join(", ", DistinctOnColumns.Select(c => c.ToSqlWithoutCte()))})";
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.Command, "distinct");
        yield return new Token(TokenType.Command, "on");
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
