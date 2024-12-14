namespace Carbunqlex.ValueExpressions;

public class ConstantExpression : IValueExpression
{
    public static ConstantExpression CreateEscapeString(string value)
    {
        return new ConstantExpression($"'{value.Replace("'", "''")}'");
    }

    public object Value { get; set; }

    public ConstantExpression(object value)
    {
        Value = value;
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => false;

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        yield return new Lexeme(LexType.Constant, Value.ToString()!);
    }

    public string ToSqlWithoutCte()
    {
        return Value.ToString()!;
    }

    public IEnumerable<IQuery> GetQueries()
    {
        // ConstantExpression does not directly use queries, so return an empty list
        return Enumerable.Empty<IQuery>();
    }
}
