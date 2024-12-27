using System.Text;

namespace Carbunqlex.ValueExpressions;

public class WhenThenPair : IValueExpression
{
    public IValueExpression When { get; }
    public IValueExpression Then { get; }

    public WhenThenPair(IValueExpression when, IValueExpression then)
    {
        When = when;
        Then = then;
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => When.MightHaveQueries || Then.MightHaveQueries;

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append("when ");
        sb.Append(When.ToSqlWithoutCte());
        sb.Append(" then ");
        sb.Append(Then.ToSqlWithoutCte());
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        yield return new Lexeme(LexType.Keyword, "when");
        foreach (var lexeme in When.GenerateLexemesWithoutCte())
        {
            yield return lexeme;
        }
        yield return new Lexeme(LexType.Keyword, "then");
        foreach (var lexeme in Then.GenerateLexemesWithoutCte())
        {
            yield return lexeme;
        }
    }

    public IEnumerable<IQuery> GetQueries()
    {
        var queries = new List<IQuery>();

        if (When.MightHaveQueries)
        {
            queries.AddRange(When.GetQueries());
        }
        if (Then.MightHaveQueries)
        {
            queries.AddRange(Then.GetQueries());
        }

        return queries;
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        var columns = new List<ColumnExpression>();

        columns.AddRange(When.ExtractColumnExpressions());
        columns.AddRange(Then.ExtractColumnExpressions());

        return columns;
    }
}
