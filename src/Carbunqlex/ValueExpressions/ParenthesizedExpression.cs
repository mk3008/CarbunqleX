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

    public bool MightHaveQueries => InnerExpression.MightHaveQueries;

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

    public IEnumerable<IQuery> GetQueries()
    {
        if (InnerExpression.MightHaveQueries)
        {
            return InnerExpression.GetQueries();
        }
        return Enumerable.Empty<IQuery>();
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        return InnerExpression.ExtractColumnExpressions();
    }
}
