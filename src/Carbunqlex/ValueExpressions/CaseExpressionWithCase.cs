using Carbunqlex.Clauses;
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

    public bool MightHaveCommonTableClauses => Case.MightHaveCommonTableClauses ||
                                               WhenThenPairs.Any(pair => pair.MightHaveCommonTableClauses) ||
                                               Else.MightHaveCommonTableClauses;

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

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        if (!MightHaveCommonTableClauses)
        {
            return Enumerable.Empty<CommonTableClause>();
        }

        var commonTableClauses = new List<CommonTableClause>();

        if (Case.MightHaveCommonTableClauses)
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
