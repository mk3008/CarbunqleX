namespace Carbunqlex.ValueExpressions;

public class InExpression : IValueExpression
{
    public IValueExpression Left { get; set; }
    public List<IValueExpression> Right { get; set; }
    public bool IsNotIn { get; set; }

    public InExpression(IValueExpression left, bool isNotIn, params IValueExpression[] right)
    {
        Left = left;
        IsNotIn = isNotIn;
        Right = right.ToList();
    }

    public string DefaultName => Left.DefaultName;

    public IEnumerable<Lexeme> GetLexemes()
    {
        foreach (var lexeme in Left.GetLexemes())
        {
            yield return lexeme;
        }
        yield return new Lexeme(LexType.Operator, IsNotIn ? "not in" : "in");
        yield return new Lexeme(LexType.OpenParen, "(");
        for (int i = 0; i < Right.Count; i++)
        {
            foreach (var lexeme in Right[i].GetLexemes())
            {
                yield return lexeme;
            }
            if (i < Right.Count - 1)
            {
                yield return new Lexeme(LexType.Comma, ",");
            }
        }
        yield return new Lexeme(LexType.CloseParen, ")");
    }

    public string ToSql()
    {
        var right = string.Join(", ", Right.Select(arg => arg.ToSql()));
        return $"{Left.ToSql()} {(IsNotIn ? "not in" : "in")} ({right})";
    }
}
