using Carbunqlex.Clauses;
using System.Text;

namespace Carbunqlex.ValueExpressions;

public class ParenthesizedExpression : IValueExpression
{
    public IValueExpression InnerExpression { get; set; }

    public ParenthesizedExpression(IValueExpression innerExpression)
    {
        InnerExpression = innerExpression;
    }

    public string DefaultName => InnerExpression.DefaultName;

    public bool MightHaveCommonTableClauses => InnerExpression.MightHaveCommonTableClauses;

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        yield return new Lexeme(LexType.OpenParen, "(");
        foreach (var lexeme in InnerExpression.GenerateLexemesWithoutCte())
        {
            yield return lexeme;
        }
        yield return new Lexeme(LexType.CloseParen, ")");
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append("(");
        sb.Append(InnerExpression.ToSqlWithoutCte());
        sb.Append(")");
        return sb.ToString();
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        if (InnerExpression.MightHaveCommonTableClauses)
        {
            return InnerExpression.GetCommonTableClauses();
        }
        return Enumerable.Empty<CommonTableClause>();
    }
}
