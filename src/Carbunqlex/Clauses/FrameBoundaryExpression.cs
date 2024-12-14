using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex.Clauses;

public class FrameBoundaryExpression : IWindowFrameBoundaryExpression
{
    public static FrameBoundaryExpression Preceding(IValueExpression rows) => new FrameBoundaryExpression(rows, "preceding");
    public static FrameBoundaryExpression Following(IValueExpression rows) => new FrameBoundaryExpression(rows, "following");

    public string BoundaryKeyword { get; }
    public IValueExpression Value { get; }

    private FrameBoundaryExpression(IValueExpression value, string boundaryKeyword)
    {
        BoundaryKeyword = boundaryKeyword;
        Value = value;
    }

    public bool MightHaveQueries => Value.MightHaveQueries;

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(Value.ToSqlWithoutCte())
          .Append(' ')
          .Append(BoundaryKeyword);
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        var lexemes = new List<Lexeme>(Value.GenerateLexemesWithoutCte().Count() + 1);
        lexemes.AddRange(Value.GenerateLexemesWithoutCte());
        lexemes.Add(new Lexeme(LexType.Keyword, BoundaryKeyword));
        return lexemes;
    }

    public IEnumerable<IQuery> GetQueries()
    {
        return Value.GetQueries();
    }
}
