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

    public string ToSqlWithoutCte()
    {
        if (Value == null)
        {
            return BoundaryKeyword;
        }

        var sb = new StringBuilder();
        sb.Append(Value.ToSqlWithoutCte())
          .Append(' ')
          .Append(BoundaryKeyword);
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        var lexemes = new List<Lexeme>(Value != null ? Value.GenerateLexemesWithoutCte().Count() + 1 : 1);
        if (Value != null)
        {
            lexemes.AddRange(Value.GenerateLexemesWithoutCte());
        }
        lexemes.Add(new Lexeme(LexType.Keyword, BoundaryKeyword));
        return lexemes;
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        if (Value == null || !Value.MightHaveCommonTableClauses)
        {
            return Enumerable.Empty<CommonTableClause>();
        }
        else
        {
            return Value.GetCommonTableClauses();
        }
    }
}
