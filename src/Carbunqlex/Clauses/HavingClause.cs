using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex.Clauses;

public class HavingClause : ISqlComponent
{
    public List<IValueExpression> Conditions { get; set; }

    public HavingClause(params IValueExpression[] conditions)
    {
        Conditions = conditions.ToList();
    }

    public string ToSql()
    {
        if (Conditions.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder("having ");
        sb.Append(string.Join(", ", Conditions.Select(c => c.ToSql())));
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        if (Conditions.Count == 0)
        {
            return Enumerable.Empty<Lexeme>();
        }

        // Estimate the initial capacity for the lexemes list.
        // Each condition can return multiple lexemes, so we add a buffer.
        // For example, a condition like "sum(a.value)" can return up to 4 lexemes:
        // "sum", "(", "a.value", ")"
        // Additionally, we add space for commas and the "having" keyword.
        int initialCapacity = Conditions.Count * 5 + 1;
        var lexemes = new List<Lexeme>(initialCapacity)
        {
            new Lexeme(LexType.StartClause, "having", "having")
        };

        foreach (var condition in Conditions)
        {
            lexemes.AddRange(condition.GetLexemes());
            lexemes.Add(new Lexeme(LexType.Comma, ",", "having"));
        }

        if (lexemes.Count > 1)
        {
            // Remove the last comma
            lexemes.RemoveAt(lexemes.Count - 1);
        }

        lexemes.Add(new Lexeme(LexType.EndClause, string.Empty, "having"));
        return lexemes;
    }
}
