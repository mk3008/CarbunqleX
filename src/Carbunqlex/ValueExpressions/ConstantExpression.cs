using System.Globalization;

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

    public static ConstantExpression Create(object? value)
    {
        string columnValue;
        if (value is DateTime dateTimeValue)
        {
            columnValue = "'" + dateTimeValue.ToString("yyyy-MM-dd HH:mm:ss") + "'";
        }
        else if (value is double doubleValue)
        {
            columnValue = doubleValue.ToString("G", CultureInfo.InvariantCulture);
        }
        else if (value is string stringValue)
        {
            columnValue = "'" + stringValue.Replace("'", "''") + "'";
        }
        else
        {
            columnValue = value?.ToString() ?? "null";
        }
        return new ConstantExpression(columnValue);
    }
}
