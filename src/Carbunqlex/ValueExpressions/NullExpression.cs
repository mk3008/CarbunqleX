namespace Carbunqlex.ValueExpressions;

public class NullExpression : IValueExpression
{
    public bool IsNotNull { get; set; }

    public NullExpression(bool isNotNull = false)
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
