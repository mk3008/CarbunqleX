using System.Text;

namespace Carbunqlex.ValueExpressions;

public class CaseExpressionWithoutCase : IValueExpression
{
    public List<WhenThenPair> WhenThenPairs { get; }
    public IValueExpression Else { get; }

    public CaseExpressionWithoutCase(List<WhenThenPair> whenThenPairs, IValueExpression elseExpression)
    {
        WhenThenPairs = whenThenPairs;
        Else = elseExpression;
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => WhenThenPairs.Any(pair => pair.When.MightHaveQueries || pair.Then.MightHaveQueries) ||
                                    Else.MightHaveQueries;

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        yield return new Lexeme(LexType.Keyword, "case");

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
}
