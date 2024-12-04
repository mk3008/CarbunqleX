namespace Carbunqlex.QueryModels;

public class NullValue : IValueExpression
{
    public bool IsNotNull { get; set; }

    public NullValue(bool isNotNull = false)
    {
        IsNotNull = isNotNull;
    }

    public string DefaultName => string.Empty;

    public IEnumerable<Lexeme> GetLexemes()
    {
        yield return new Lexeme(LexType.Keyword, IsNotNull ? "not null" : "null");
    }

    public string ToSql()
    {
        return IsNotNull ? "not null" : "null";
    }
}
