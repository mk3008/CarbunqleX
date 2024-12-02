using System.Text;

namespace Carbunqlex.QueryModels;

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

public class WhenThenPair
{
    public IValueExpression When { get; }
    public IValueExpression Then { get; }

    public WhenThenPair(IValueExpression when, IValueExpression then)
    {
        When = when;
        Then = then;
    }

    public string ToSql()
    {
        return $"when {When.ToSql()} then {Then.ToSql()}";
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        yield return new Lexeme(LexType.Keyword, "when");
        foreach (var lexeme in When.GetLexemes())
        {
            yield return lexeme;
        }
        yield return new Lexeme(LexType.Keyword, "then");
        foreach (var lexeme in Then.GetLexemes())
        {
            yield return lexeme;
        }
    }
}
