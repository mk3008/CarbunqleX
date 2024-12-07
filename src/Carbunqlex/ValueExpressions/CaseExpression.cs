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

    public IEnumerable<Lexeme> GetLexemes()
    {
        yield return new Lexeme(LexType.Keyword, "case");

        if (Case != null)
        {
            foreach (var lexeme in Case.GetLexemes())
            {
                yield return lexeme;
            }
        }

        foreach (var pair in WhenThenPairs)
        {
            foreach (var lexeme in pair.GetLexemes())
            {
                yield return lexeme;
            }
        }

        if (Else != null)
        {
            yield return new Lexeme(LexType.Keyword, "else");
            foreach (var lexeme in Else.GetLexemes())
            {
                yield return lexeme;
            }
        }

        yield return new Lexeme(LexType.Keyword, "end");
    }

    public string ToSql()
    {
        var sql = new StringBuilder("case");

        if (Case != null)
        {
            sql.Append($" {Case.ToSql()}");
        }

        foreach (var pair in WhenThenPairs)
        {
            sql.Append($" {pair.ToSql()}");
        }

        if (Else != null)
        {
            sql.Append($" else {Else.ToSql()}");
        }

        sql.Append(" end");
        return sql.ToString();
    }
}
