using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex.Clauses;

public class WindowFrameBoundaryExpression : ISqlComponent
{
    public string BoundaryKeyword { get; }
    public IValueExpression? Value { get; }

    private WindowFrameBoundaryExpression(string boundaryKeyword)
    {
        BoundaryKeyword = boundaryKeyword;
        Value = null;
    }

    private WindowFrameBoundaryExpression(IValueExpression value, string boundaryKeyword)
    {
        BoundaryKeyword = boundaryKeyword;
        Value = value;
    }

    public static WindowFrameBoundaryExpression UnboundedPreceding => new WindowFrameBoundaryExpression("unbounded preceding");
    public static WindowFrameBoundaryExpression CurrentRow => new WindowFrameBoundaryExpression("current row");
    public static WindowFrameBoundaryExpression UnboundedFollowing => new WindowFrameBoundaryExpression("unbounded following");
    public static WindowFrameBoundaryExpression Preceding(IValueExpression rows) => new WindowFrameBoundaryExpression(rows, "preceding");
    public static WindowFrameBoundaryExpression Following(IValueExpression rows) => new WindowFrameBoundaryExpression(rows, "following");

    public string ToSql()
    {
        if (Value == null)
        {
            return BoundaryKeyword;
        }

        var sb = new StringBuilder();
        sb.Append(Value.ToSql())
          .Append(' ')
          .Append(BoundaryKeyword);
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        var lexemes = new List<Lexeme>(Value != null ? Value.GetLexemes().Count() + 1 : 1);
        if (Value != null)
        {
            lexemes.AddRange(Value.GetLexemes());
        }
        lexemes.Add(new Lexeme(LexType.Keyword, BoundaryKeyword));
        return lexemes;
    }
}
