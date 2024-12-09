using Carbunqlex.Clauses;

namespace Carbunqlex.ValueExpressions;

public class ParameterExpression : IValueExpression
{
    public string Name { get; }
    public object? Value { get; }
    public bool MightHaveCommonTableClauses => false;

    public ParameterExpression(string name, object? value = null)
    {
        Name = name;
        Value = value;
    }

    public string ToSqlWithoutCte()
    {
        return Name;
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        return new List<Lexeme>
        {
            new Lexeme(LexType.Parameter, Name)
        };
    }

    public string DefaultName => string.Empty;

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        // ParameterExpression does not directly use CTEs, so return an empty list
        return Enumerable.Empty<CommonTableClause>();
    }
}
