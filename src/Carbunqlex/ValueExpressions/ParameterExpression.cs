namespace Carbunqlex.ValueExpressions;

public class ParameterExpression : IValueExpression
{
    public string Name { get; }
    public object? Value { get; }

    public ParameterExpression(string name, object? value = null)
    {
        Name = name;
        Value = value;
    }

    public string ToSql()
    {
        return Name;
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        return new List<Lexeme>
        {
            new Lexeme(LexType.Parameter, Name)
        };
    }

    public string DefaultName => Name;
}
