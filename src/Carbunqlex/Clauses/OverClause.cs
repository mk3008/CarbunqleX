using System.Text;

namespace Carbunqlex.Clauses;

public class OverClause : ISqlComponent
{
    public WindowFunction? WindowFunction { get; set; }

    public OverClause(WindowFunction? windowFunction = null)
    {
        WindowFunction = windowFunction;
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append("over (");
        if (WindowFunction != null)
        {
            sb.Append(WindowFunction.ToSqlWithoutCte());
        }
        sb.Append(")");
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        var lexemes = new List<Lexeme>
        {
            new Lexeme(LexType.StartClause, "over", "over"),
            new Lexeme(LexType.OpenParen, "(", "over")
        };

        if (WindowFunction != null)
        {
            lexemes.AddRange(WindowFunction.GenerateLexemesWithoutCte());
        }

        lexemes.Add(new Lexeme(LexType.CloseParen, ")", "over"));
        lexemes.Add(new Lexeme(LexType.EndClause, string.Empty, "over"));
        return lexemes;
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        if (WindowFunction == null)
        {
            return Enumerable.Empty<CommonTableClause>();
        }
        return WindowFunction.GetCommonTableClauses();
    }
}
