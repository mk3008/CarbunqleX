using System.Text;

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

    public bool MightHaveQueries => Left.MightHaveQueries || Right.MightHaveQueries;

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        foreach (var lexeme in Left.GenerateLexemesWithoutCte())
        {
            yield return lexeme;
        }
        yield return new Lexeme(LexType.Operator, IsNotLike ? "not like" : "like");
        foreach (var lexeme in Right.GenerateLexemesWithoutCte())
        {
            yield return lexeme;
        }
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(Left.ToSqlWithoutCte());
        sb.Append(IsNotLike ? " not like " : " like ");
        sb.Append(Right.ToSqlWithoutCte());
        return sb.ToString();
    }

    public IEnumerable<IQuery> GetQueries()
    {
        var queries = new List<IQuery>();

        if (Left.MightHaveQueries)
        {
            queries.AddRange(Left.GetQueries());
        }
        if (Right.MightHaveQueries)
        {
            queries.AddRange(Right.GetQueries());
        }

        return queries;
    }
}
