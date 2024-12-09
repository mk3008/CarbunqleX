using Carbunqlex.Clauses;
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

    public bool MightHaveCommonTableClauses => Left.MightHaveCommonTableClauses || Right.MightHaveCommonTableClauses;

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

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        if (!MightHaveCommonTableClauses)
        {
            return Enumerable.Empty<CommonTableClause>();
        }

        var commonTableClauses = new List<CommonTableClause>();

        if (Left.MightHaveCommonTableClauses)
        {
            commonTableClauses.AddRange(Left.GetCommonTableClauses());
        }
        if (Right.MightHaveCommonTableClauses)
        {
            commonTableClauses.AddRange(Right.GetCommonTableClauses());
        }

        return commonTableClauses;
    }
}
