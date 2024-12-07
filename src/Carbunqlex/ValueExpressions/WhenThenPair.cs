using System.Text;

namespace Carbunqlex.ValueExpressions;

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
        var sb = new StringBuilder();
        sb.Append("when ");
        sb.Append(When.ToSql());
        sb.Append(" then ");
        sb.Append(Then.ToSql());
        return sb.ToString();
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
