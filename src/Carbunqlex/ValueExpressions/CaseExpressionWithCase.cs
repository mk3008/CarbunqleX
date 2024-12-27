using System.Text;

namespace Carbunqlex.ValueExpressions;

public class CaseExpressionWithCase : IValueExpression
{
    public IValueExpression Case { get; }
    public List<WhenThenPair> WhenThenPairs { get; }
    public IValueExpression Else { get; }

    public CaseExpressionWithCase(IValueExpression caseExpression, List<WhenThenPair> whenThenPairs, IValueExpression elseExpression)
    {
        Case = caseExpression;
        WhenThenPairs = whenThenPairs;
        Else = elseExpression;
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => Case.MightHaveQueries ||
                                    WhenThenPairs.Any(pair => pair.When.MightHaveQueries || pair.Then.MightHaveQueries) ||
                                    Else.MightHaveQueries;

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        yield return new Lexeme(LexType.Keyword, "case");

        foreach (var lexeme in Case.GenerateLexemesWithoutCte())
        {
            yield return lexeme;
        }

        foreach (var pair in WhenThenPairs)
        {
            foreach (var lexeme in pair.GenerateLexemesWithoutCte())
            {
                yield return lexeme;
            }
        }

        yield return new Lexeme(LexType.Keyword, "else");
        foreach (var lexeme in Else.GenerateLexemesWithoutCte())
        {
            yield return lexeme;
        }

        yield return new Lexeme(LexType.Keyword, "end");
    }

    public string ToSqlWithoutCte()
    {
        var sql = new StringBuilder("case");
        sql.Append($" {Case.ToSqlWithoutCte()}");

        foreach (var pair in WhenThenPairs)
        {
            sql.Append($" {pair.ToSqlWithoutCte()}");
        }

        sql.Append($" else {Else.ToSqlWithoutCte()}");
        sql.Append(" end");
        return sql.ToString();
    }

    public IEnumerable<IQuery> GetQueries()
    {
        var queries = new List<IQuery>();

        if (Case.MightHaveQueries)
        {
            queries.AddRange(Case.GetQueries());
        }

        foreach (var pair in WhenThenPairs)
        {
            if (pair.When.MightHaveQueries)
            {
                queries.AddRange(pair.When.GetQueries());
            }
            if (pair.Then.MightHaveQueries)
            {
                queries.AddRange(pair.Then.GetQueries());
            }
        }

        if (Else.MightHaveQueries)
        {
            queries.AddRange(Else.GetQueries());
        }

        return queries;
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        var columns = new List<ColumnExpression>();

        columns.AddRange(Case.ExtractColumnExpressions());

        foreach (var pair in WhenThenPairs)
        {
            columns.AddRange(pair.ExtractColumnExpressions());
        }

        columns.AddRange(Else.ExtractColumnExpressions());

        return columns;
    }
}
