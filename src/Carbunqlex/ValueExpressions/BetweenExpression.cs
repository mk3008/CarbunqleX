using System.Text;

namespace Carbunqlex.ValueExpressions;

public class BetweenExpression : IValueExpression
{
    public IValueExpression Left { get; set; }
    public IValueExpression Start { get; set; }
    public IValueExpression End { get; set; }
    public bool IsNegated { get; set; }

    public BetweenExpression(bool isNegated, IValueExpression left, IValueExpression start, IValueExpression end)
    {
        Left = left;
        IsNegated = isNegated;
        Start = start;
        End = end;
    }

    public string DefaultName => Left.DefaultName;

    public bool MightHaveQueries => Left.MightHaveQueries || Start.MightHaveQueries || End.MightHaveQueries;

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        foreach (var lexeme in Left.GenerateTokensWithoutCte())
        {
            yield return lexeme;
        }
        yield return new Token(TokenType.Operator, IsNegated ? "not between" : "between");
        foreach (var lexeme in Start.GenerateTokensWithoutCte())
        {
            yield return lexeme;
        }
        yield return new Token(TokenType.Operator, "and");
        foreach (var lexeme in End.GenerateTokensWithoutCte())
        {
            yield return lexeme;
        }
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(Left.ToSqlWithoutCte());
        sb.Append(IsNegated ? " not between " : " between ");
        sb.Append(Start.ToSqlWithoutCte());
        sb.Append(" and ");
        sb.Append(End.ToSqlWithoutCte());
        return sb.ToString();
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        if (!MightHaveQueries)
        {
            return Enumerable.Empty<ISelectQuery>();
        }

        var queries = new List<ISelectQuery>();

        if (Left.MightHaveQueries)
        {
            queries.AddRange(Left.GetQueries());
        }
        if (Start.MightHaveQueries)
        {
            queries.AddRange(Start.GetQueries());
        }
        if (End.MightHaveQueries)
        {
            queries.AddRange(End.GetQueries());
        }

        return queries;
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        var columns = new List<ColumnExpression>();
        columns.AddRange(Left.ExtractColumnExpressions());
        columns.AddRange(Start.ExtractColumnExpressions());
        columns.AddRange(End.ExtractColumnExpressions());
        return columns;
    }
}
