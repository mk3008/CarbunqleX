using System.Text;

namespace Carbunqlex.Clauses;

public class WithClause : ISqlComponent
{
    public List<CommonTableClause> CommonTableClauses { get; }
    public bool IsRecursive { get; set; }

    public WithClause(bool isRecursive, params CommonTableClause[] commonTableClauses)
    {
        IsRecursive = isRecursive;
        CommonTableClauses = commonTableClauses.ToList();
    }

    public WithClause(params CommonTableClause[] commonTableClauses)
        : this(false, commonTableClauses)
    {
    }

    public string ToSql()
    {
        if (CommonTableClauses.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.Append("with ");
        if (IsRecursive)
        {
            sb.Append("recursive ");
        }
        sb.Append(string.Join(", ", CommonTableClauses.Select(cte => cte.ToSql())));
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        if (CommonTableClauses.Count == 0)
        {
            return Enumerable.Empty<Lexeme>();
        }

        var lexemes = new List<Lexeme>();

        if (IsRecursive)
        {
            lexemes.Add(new Lexeme(LexType.StartClause, "with recursive", "with"));
        }
        else
        {
            lexemes.Add(new Lexeme(LexType.StartClause, "with", "with"));
        }

        foreach (var cte in CommonTableClauses)
        {
            lexemes.AddRange(cte.GetLexemes());
            lexemes.Add(Lexeme.Comma);
        }

        if (lexemes.Count > 1)
        {
            // Remove the last comma
            lexemes.RemoveAt(lexemes.Count - 1);
        }

        lexemes.Add(new Lexeme(LexType.EndClause, string.Empty, "with"));

        return lexemes;
    }
}
