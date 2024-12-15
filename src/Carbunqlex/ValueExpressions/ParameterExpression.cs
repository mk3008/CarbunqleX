namespace Carbunqlex.ValueExpressions;

public class ParameterExpression : IValueExpression
{
    public string Name { get; }
    public bool MightHaveQueries => false;

    public ParameterExpression(string name)
    {
        Name = name;
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

    public IEnumerable<IQuery> GetQueries()
    {
        // ParameterExpression does not directly use queries, so return an empty list
        return Enumerable.Empty<IQuery>();
    }
}
