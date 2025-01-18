namespace Carbunqlex.ValueExpressions;

public class NullExpression : IValueExpression
{
    public bool IsNegated { get; set; }
    public bool MightHaveQueries => false;

    public NullExpression(bool isNegated = false)
    {
        IsNegated = isNegated;
    }

    public string DefaultName => string.Empty;

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.Keyword, IsNegated ? "not null" : "null");
    }

    public string ToSqlWithoutCte()
    {
        return IsNegated ? "not null" : "null";
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        // NullExpression does not directly use queries, so return an empty list
        return Enumerable.Empty<ISelectQuery>();
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        // NullExpression does not have columns, so return an empty list
        return Enumerable.Empty<ColumnExpression>();
    }
}
