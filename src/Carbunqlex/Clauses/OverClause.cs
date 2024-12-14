using System.Text;

namespace Carbunqlex.Clauses;

public class OverClause : IOverClause
{
    public IWindowFunction WindowFunction { get; set; }

    public bool MightHaveCommonTableClauses => WindowFunction.MightHaveCommonTableClauses;

    public OverClause(IWindowFunction windowFunction)
    {
        WindowFunction = windowFunction;
    }

    public OverClause() : this(EmptyWindowFunction.Instance)
    {
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append("over (");
        sb.Append(WindowFunction.ToSqlWithoutCte());
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

        lexemes.AddRange(WindowFunction.GenerateLexemesWithoutCte());

        lexemes.Add(new Lexeme(LexType.CloseParen, ")", "over"));
        lexemes.Add(new Lexeme(LexType.EndClause, string.Empty, "over"));
        return lexemes;
    }

    public IEnumerable<IQuery> GetQueries()
    {
        return WindowFunction.GetQueries();
    }
}
