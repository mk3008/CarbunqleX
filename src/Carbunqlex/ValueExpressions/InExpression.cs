using Carbunqlex.Lexing;
using System.Text;

namespace Carbunqlex.ValueExpressions;

public class InExpression : IValueExpression
{
    public IValueGroupExpression Left { get; set; }
    public IValueGroupExpression Right { get; set; }
    public bool IsNegated { get; set; }

    public InExpression(bool isNegated, IValueExpression left, IValueGroupExpression right)
    {
        IsNegated = isNegated;
        Left = new InValueGroupExpression(new List<IValueExpression>() { left });
        Right = right;
    }

    public InExpression(bool isNegated, IValueGroupExpression left, IValueGroupExpression right)
    {
        IsNegated = isNegated;
        Left = left;
        Right = right;
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => Left.MightHaveQueries || Right.MightHaveQueries;

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        foreach (var lexeme in Left.GenerateTokensWithoutCte())
        {
            yield return lexeme;
        }
        yield return new Token(TokenType.Operator, IsNegated ? "not in" : "in");
        foreach (var lexeme in Right.GenerateTokensWithoutCte())
        {
            yield return lexeme;
        }
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(Left.ToSqlWithoutCte());
        sb.Append(IsNegated ? " not in " : " in ");
        sb.Append(Right.ToSqlWithoutCte());
        return sb.ToString();
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery>();

        if (Left.MightHaveQueries)
        {
            queries.AddRange(Left.GetQueries());
        }
        if (Right.MightHaveQueries)
        {
            queries.AddRange(Right.GetQueries());
        }

        return queries;
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        var columns = new List<ColumnExpression>();

        columns.AddRange(Left.ExtractColumnExpressions());
        columns.AddRange(Right.ExtractColumnExpressions());

        return columns;
    }
}
