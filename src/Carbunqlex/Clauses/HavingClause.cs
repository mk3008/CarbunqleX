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

    public string ToSqlWithoutCte()
    {
        if (Conditions.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder("having ");
        sb.Append(string.Join(", ", Conditions.Select(c => c.ToSqlWithoutCte())));
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        if (Conditions.Count == 0)
        {
            return Enumerable.Empty<Lexeme>();
        }

        int initialCapacity = Conditions.Count * 5 + 1;
        var lexemes = new List<Lexeme>(initialCapacity)
        {
            new Lexeme(LexType.StartClause, "having", "having")
        };

        foreach (var condition in Conditions)
        {
            lexemes.AddRange(condition.GenerateLexemesWithoutCte());
            lexemes.Add(new Lexeme(LexType.Comma, ",", "having"));
        }

        if (lexemes.Count > 1)
        {
            lexemes.RemoveAt(lexemes.Count - 1);
        }

        lexemes.Add(new Lexeme(LexType.EndClause, string.Empty, "having"));
        return lexemes;
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        return Conditions
            .Where(condition => condition.MightHaveCommonTableClauses)
            .SelectMany(condition => condition.GetCommonTableClauses());
    }
}
