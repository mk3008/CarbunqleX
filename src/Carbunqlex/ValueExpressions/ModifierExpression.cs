namespace Carbunqlex.ValueExpressions;

/// <summary>
/// Represents a SQL modifier expression, such as INTERVAL '1 day' or DATE '2024-12-25'.
public class ModifierExpression : IValueExpression
{
    public string Modifier { get; }
    public IValueExpression Value { get; }

    public ModifierExpression(string modifier, IValueExpression value)
    {
        Modifier = modifier;
        Value = value;
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => Value.MightHaveQueries;

    public string ToSqlWithoutCte()
    {
        return $"{Modifier} {Value.ToSqlWithoutCte()}";
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        yield return new Lexeme(LexType.Keyword, Modifier);
        foreach (var lexeme in Value.GenerateLexemesWithoutCte())
        {
            yield return lexeme;
        }
    }

    public IEnumerable<IQuery> GetQueries()
    {
        return Value.GetQueries();
    }
}
