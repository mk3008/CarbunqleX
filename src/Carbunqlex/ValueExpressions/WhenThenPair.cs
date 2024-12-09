using Carbunqlex.Clauses;
using System.Text;

namespace Carbunqlex.ValueExpressions;

public class WhenThenPair : ISqlComponent
{
    public IValueExpression When { get; }
    public IValueExpression Then { get; }

    public WhenThenPair(IValueExpression when, IValueExpression then)
    {
        When = when;
        Then = then;
    }

    public bool MightHaveCommonTableClauses => When.MightHaveCommonTableClauses || Then.MightHaveCommonTableClauses;

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

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        if (!MightHaveCommonTableClauses)
        {
            return Enumerable.Empty<CommonTableClause>();
        }

        var commonTableClauses = new List<CommonTableClause>();

        if (When.MightHaveCommonTableClauses)
        {
            commonTableClauses.AddRange(When.GetCommonTableClauses());
        }
        if (Then.MightHaveCommonTableClauses)
        {
            commonTableClauses.AddRange(Then.GetCommonTableClauses());
        }

        return commonTableClauses;
    }
}
