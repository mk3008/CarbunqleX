using Carbunqlex.Clauses;
using System.Text;

namespace Carbunqlex.ValueExpressions;

public class CaseExpression : IValueExpression
{
    public IValueExpression? Case { get; }
    public List<WhenThenPair> WhenThenPairs { get; }
    public IValueExpression Else { get; }

    public CaseExpression(List<WhenThenPair> whenThenPairs, IValueExpression elseExpression)
    {
        Case = null;
        WhenThenPairs = whenThenPairs;
        Else = elseExpression;
    }

    public CaseExpression(IValueExpression caseExpression, List<WhenThenPair> whenThenPairs, IValueExpression elseExpression)
    {
        Case = caseExpression;
        WhenThenPairs = whenThenPairs;
        Else = elseExpression;
    }

    public string DefaultName => string.Empty;

    public bool MightHaveCommonTableClauses => (Case?.MightHaveCommonTableClauses ?? false) ||
                                               WhenThenPairs.Any(pair => pair.MightHaveCommonTableClauses) ||
                                               Else.MightHaveCommonTableClauses;

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        yield return new Lexeme(LexType.Keyword, "case");

        if (Case != null)
        {
            foreach (var lexeme in Case.GenerateLexemesWithoutCte())
            {
                yield return lexeme;
            }
        }

        foreach (var pair in WhenThenPairs)
        {
            foreach (var lexeme in pair.GenerateLexemesWithoutCte())
            {
                yield return lexeme;
            }
        }

        if (Else != null)
        {
            yield return new Lexeme(LexType.Keyword, "else");
            foreach (var lexeme in Else.GenerateLexemesWithoutCte())
            {
                yield return lexeme;
            }
        }

        yield return new Lexeme(LexType.Keyword, "end");
    }

    public string ToSqlWithoutCte()
    {
        var sql = new StringBuilder("case");

        if (Case != null)
        {
            sql.Append($" {Case.ToSqlWithoutCte()}");
        }

        foreach (var pair in WhenThenPairs)
        {
            sql.Append($" {pair.ToSqlWithoutCte()}");
        }

        if (Else != null)
        {
            sql.Append($" else {Else.ToSqlWithoutCte()}");
        }

        sql.Append(" end");
        return sql.ToString();
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        if (!MightHaveCommonTableClauses)
        {
            return Enumerable.Empty<CommonTableClause>();
        }

        var commonTableClauses = new List<CommonTableClause>();

        if (Case?.MightHaveCommonTableClauses == true)
        {
            commonTableClauses.AddRange(Case.GetCommonTableClauses());
        }

        foreach (var pair in WhenThenPairs)
        {
            if (pair.MightHaveCommonTableClauses)
            {
                commonTableClauses.AddRange(pair.GetCommonTableClauses());
            }
        }

        if (Else.MightHaveCommonTableClauses)
        {
            commonTableClauses.AddRange(Else.GetCommonTableClauses());
        }

        return commonTableClauses;
    }
}
