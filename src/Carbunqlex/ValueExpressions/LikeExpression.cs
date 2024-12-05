namespace Carbunqlex.ValueExpressions;

public class LikeExpression : IValueExpression
{
    public IValueExpression Left { get; set; }
    public IValueExpression Right { get; set; }
    public bool IsNotLike { get; set; }
    public LikeExpression(IValueExpression left, bool isNotLike, IValueExpression right)
    {
        Left = left;
        IsNotLike = isNotLike;
        Right = right;
    }
    public string DefaultName => Left.DefaultName;
    public IEnumerable<Lexeme> GetLexemes()
    {
        foreach (var lexeme in Left.GetLexemes())
        {
            yield return lexeme;
        }
        yield return new Lexeme(LexType.Operator, IsNotLike ? "not like" : "like");
        foreach (var lexeme in Right.GetLexemes())
        {
            yield return lexeme;
        }
    }
    public string ToSql()
    {
        return $"{Left.ToSql()} {(IsNotLike ? "not like" : "like")} {Right.ToSql()}";
    }
}
