namespace Carbunqlex.ValueExpressions;

/// <summary>
/// Represents a keyword in a SQL query.
/// e.g. CURRENT_DATE, CURRENT_TIME, CURRENT_TIMESTAMP etc.
/// </summary>
public class KeywordExpression : IValueExpression
{
    public string Keyword { get; }

    public KeywordExpression(string keyword)
    {
        Keyword = keyword;
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => false;

    public string ToSqlWithoutCte()
    {
        return Keyword;
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.Command, Keyword);
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return Enumerable.Empty<ISelectQuery>();
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        return Enumerable.Empty<ColumnExpression>();
    }
}
