using System.Text;

namespace Carbunqlex.ValueExpressions;

public class UnaryExpression : IValueExpression
{
    public string Operator { get; set; }
    public IValueExpression Operand { get; set; }

    public UnaryExpression(string @operator, IValueExpression operand)
    {
        Operator = @operator;
        Operand = operand;
    }

    public string DefaultName => Operand.DefaultName;

    public bool MightHaveQueries => Operand.MightHaveQueries;

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        yield return new Lexeme(LexType.Operator, Operator);
        foreach (var lexeme in Operand.GenerateLexemesWithoutCte())
        {
            yield return lexeme;
        }
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(Operator);
        sb.Append(" ");
        sb.Append(Operand.ToSqlWithoutCte());
        return sb.ToString();
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        if (Operand.MightHaveQueries)
        {
            return Operand.GetQueries();
        }
        return Enumerable.Empty<ISelectQuery>();
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        return Operand.ExtractColumnExpressions();
    }
}
