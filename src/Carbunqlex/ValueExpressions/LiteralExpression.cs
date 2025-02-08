namespace Carbunqlex.ValueExpressions;

public class LiteralExpression : IValueExpression
{
    public object Value { get; set; }

    public LiteralExpression(object value)
    {
        Value = value;
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => false;

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.Literal, Value.ToString()!);
    }

    public string ToSqlWithoutCte()
    {
        return Value.ToString()!;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        // ConstantExpression does not directly use queries, so return an empty list
        return Enumerable.Empty<ISelectQuery>();
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        // ConstantExpression does not have columns, so return an empty list
        return Enumerable.Empty<ColumnExpression>();
    }
}
