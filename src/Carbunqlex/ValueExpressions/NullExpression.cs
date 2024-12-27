namespace Carbunqlex.ValueExpressions;

public class NullExpression : IValueExpression
{
    public bool IsNotNull { get; set; }
    public bool MightHaveQueries => false;

    public NullExpression(bool isNotNull = false)
    {
        IsNotNull = isNotNull;
    }

    public string DefaultName => string.Empty;

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        yield return new Lexeme(LexType.Keyword, IsNotNull ? "not null" : "null");
    }

    public string ToSqlWithoutCte()
    {
        return IsNotNull ? "not null" : "null";
    }

    public IEnumerable<IQuery> GetQueries()
    {
        // NullExpression does not directly use queries, so return an empty list
        return Enumerable.Empty<IQuery>();
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        // NullExpression does not have columns, so return an empty list
        return Enumerable.Empty<ColumnExpression>();
    }
}
