namespace Carbunqlex.QueryModels;

public class BetweenExpression : IValueExpression
{
    public IValueExpression Left { get; set; }
    public IValueExpression Start { get; set; }
    public IValueExpression End { get; set; }
    public bool IsNotBetween { get; set; }
    public BetweenExpression(IValueExpression left, bool isNotBetween, IValueExpression start, IValueExpression end)
    {
        Left = left;
        IsNotBetween = isNotBetween;
        Start = start;
        End = end;
    }
    public string DefaultName => Left.DefaultName;
    public IEnumerable<Lexeme> GetLexemes()
    {
        foreach (var lexeme in Left.GetLexemes())
        {
            yield return lexeme;
        }
        yield return new Lexeme(LexType.Operator, IsNotBetween ? "not between" : "between");
        foreach (var lexeme in Start.GetLexemes())
        {
            yield return lexeme;
        }
        yield return new Lexeme(LexType.Operator, "and");
        foreach (var lexeme in End.GetLexemes())
        {
            yield return lexeme;
        }
    }
    public string ToSql()
    {
        return $"{Left.ToSql()} {(IsNotBetween ? "not between" : "between")} {Start.ToSql()} and {End.ToSql()}";
    }
}
