using System.Text;

namespace Carbunqlex.ValueExpressions;

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

    public bool MightHaveQueries => Left.MightHaveQueries || Start.MightHaveQueries || End.MightHaveQueries;

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        foreach (var lexeme in Left.GenerateLexemesWithoutCte())
        {
            yield return lexeme;
        }
        yield return new Lexeme(LexType.Operator, IsNotBetween ? "not between" : "between");
        foreach (var lexeme in Start.GenerateLexemesWithoutCte())
        {
            yield return lexeme;
        }
        yield return new Lexeme(LexType.Operator, "and");
        foreach (var lexeme in End.GenerateLexemesWithoutCte())
        {
            yield return lexeme;
        }
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(Left.ToSqlWithoutCte());
        sb.Append(IsNotBetween ? " not between " : " between ");
        sb.Append(Start.ToSqlWithoutCte());
        sb.Append(" and ");
        sb.Append(End.ToSqlWithoutCte());
        return sb.ToString();
    }

    public IEnumerable<IQuery> GetQueries()
    {
        if (!MightHaveQueries)
        {
            return Enumerable.Empty<IQuery>();
        }

        var queries = new List<IQuery>();

        if (Left.MightHaveQueries)
        {
            queries.AddRange(Left.GetQueries());
        }
        if (Start.MightHaveQueries)
        {
            queries.AddRange(Start.GetQueries());
        }
        if (End.MightHaveQueries)
        {
            queries.AddRange(End.GetQueries());
        }

        return queries;
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        var columns = new List<ColumnExpression>();
        columns.AddRange(Left.ExtractColumnExpressions());
        columns.AddRange(Start.ExtractColumnExpressions());
        columns.AddRange(End.ExtractColumnExpressions());
        return columns;
    }
}
