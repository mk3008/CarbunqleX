using System.Text;

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

    public bool MightHaveQueries => Left.MightHaveQueries || Right.Any(r => r.MightHaveQueries);

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        foreach (var lexeme in Left.GenerateLexemesWithoutCte())
        {
            yield return lexeme;
        }
        yield return new Lexeme(LexType.Operator, IsNotIn ? "not in" : "in");
        yield return new Lexeme(LexType.OpenParen, "(");
        for (int i = 0; i < Right.Count; i++)
        {
            foreach (var lexeme in Right[i].GenerateLexemesWithoutCte())
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

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(Left.ToSqlWithoutCte());
        sb.Append(IsNotIn ? " not in (" : " in (");
        sb.Append(string.Join(", ", Right.Select(arg => arg.ToSqlWithoutCte())));
        sb.Append(")");
        return sb.ToString();
    }

    public IEnumerable<IQuery> GetQueries()
    {
        var queries = new List<IQuery>();

        if (Left.MightHaveQueries)
        {
            queries.AddRange(Left.GetQueries());
        }
        foreach (var right in Right)
        {
            if (right.MightHaveQueries)
            {
                queries.AddRange(right.GetQueries());
            }
        }

        return queries;
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        var columns = new List<ColumnExpression>();

        columns.AddRange(Left.ExtractColumnExpressions());
        foreach (var right in Right)
        {
            columns.AddRange(right.ExtractColumnExpressions());
        }

        return columns;
    }
}
