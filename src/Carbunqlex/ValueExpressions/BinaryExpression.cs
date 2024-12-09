using Carbunqlex.Clauses;
using System.Text;

namespace Carbunqlex.ValueExpressions;

/// <summary>
/// Represents a binary expression, which consists of a left operand, an operator, and a right operand.
/// This class can be used to represent both arithmetic and logical expressions.
/// </summary>
public class BinaryExpression : IValueExpression
{
    public string Operator { get; set; }
    public IValueExpression Left { get; set; }
    public IValueExpression Right { get; set; }

    public BinaryExpression(string @operator, IValueExpression left, IValueExpression right)
    {
        Operator = @operator;
        Left = left;
        Right = right;
    }

    public string DefaultName => Left.DefaultName;

    public bool MightHaveCommonTableClauses => Left.MightHaveCommonTableClauses || Right.MightHaveCommonTableClauses;

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        foreach (var lexeme in Left.GenerateLexemesWithoutCte())
        {
            yield return lexeme;
        }
        yield return new Lexeme(LexType.Operator, Operator);
        foreach (var lexeme in Right.GenerateLexemesWithoutCte())
        {
            yield return lexeme;
        }
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(Left.ToSqlWithoutCte());
        sb.Append(" ");
        sb.Append(Operator);
        sb.Append(" ");
        sb.Append(Right.ToSqlWithoutCte());
        return sb.ToString();
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        if (!MightHaveCommonTableClauses)
        {
            return Enumerable.Empty<CommonTableClause>();
        }

        var commonTableClauses = new List<CommonTableClause>();

        if (Left.MightHaveCommonTableClauses)
        {
            commonTableClauses.AddRange(Left.GetCommonTableClauses());
        }
        if (Right.MightHaveCommonTableClauses)
        {
            commonTableClauses.AddRange(Right.GetCommonTableClauses());
        }

        return commonTableClauses;
    }
}
