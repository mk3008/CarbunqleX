namespace Carbunqlex.ValueExpressions;

/// <summary>
/// Represents a SQL CAST expression, which converts a value from one data type to another.
/// </summary>
public class CastExpression : IValueExpression
{
    public IValueExpression Expression { get; }
    public string TargetType { get; }

    public CastExpression(IValueExpression expression, string targetType)
    {
        Expression = expression;
        TargetType = targetType;
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => Expression.MightHaveQueries;

    public string ToSqlWithoutCte()
    {
        return $"CAST({Expression.ToSqlWithoutCte()} AS {TargetType})";
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        yield return new Lexeme(LexType.Keyword, "CAST");
        yield return new Lexeme(LexType.OpenParen, "(");
        foreach (var lexeme in Expression.GenerateLexemesWithoutCte())
        {
            yield return lexeme;
        }
        yield return new Lexeme(LexType.Keyword, "AS");
        yield return new Lexeme(LexType.Identifier, TargetType);
        yield return new Lexeme(LexType.CloseParen, ")");
    }

    public IEnumerable<IQuery> GetQueries()
    {
        return Expression.GetQueries();
    }

    public override string ToString()
    {
        return $"CAST({Expression} AS {TargetType})";
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        return Expression.ExtractColumnExpressions();
    }
}
