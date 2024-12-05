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

    public IEnumerable<Lexeme> GetLexemes()
    {
        yield return new Lexeme(LexType.Constant, Value.ToString()!);
    }

    public string ToSql()
    {
        return Value.ToString()!;
    }
}
