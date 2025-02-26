using Carbunqlex.Clauses;
using Carbunqlex.Lexing;

namespace Carbunqlex.ValueExpressions;

/// <summary>
/// Represents an inline query.
/// </summary>
public class InlineQuery : IValueExpression
{
    public ISelectQuery Query { get; }
    public string DefaultName => string.Empty;
    public bool MightHaveCommonTableClauses => true;
    public bool MightHaveQueries => true;

    public InlineQuery(ISelectQuery query)
    {
        Query = query;
    }

    public string ToSqlWithoutCte()
    {
        return $"({Query.ToSqlWithoutCte()})";
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>
        {
            new Token(TokenType.OpenParen, "(", "inline_query")
        };

        tokens.AddRange(Query.GenerateTokensWithoutCte());

        tokens.Add(new Token(TokenType.CloseParen, ")", "inline_query"));

        return tokens;
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        return Query.GetCommonTableClauses();
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return new List<ISelectQuery> { Query };
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        return Enumerable.Empty<ColumnExpression>();
    }
}
