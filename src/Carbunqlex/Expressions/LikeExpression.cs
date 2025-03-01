using Carbunqlex.Lexing;
using System.Text;

namespace Carbunqlex.Expressions;

/// <summary>
/// Represents a Like expression
/// </summary>
public class LikeExpression : IValueExpression
{
    public IValueExpression Left { get; set; }
    public IValueExpression Right { get; set; }
    public bool IsNegated { get; set; }
    public string EscapeOption { get; set; }

    public string Keyword { get; }

    /// <summary>
    /// Constructor for Like expression
    /// </summary>
    /// <param name="isNegated"></param>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <param name="escapeOption"></param>
    public LikeExpression(bool isNegated, IValueExpression left, IValueExpression right, string escapeOption = "")
    {
        Keyword = "like";
        IsNegated = isNegated;
        Left = left;
        Right = right;
        EscapeOption = escapeOption;
    }

    /// <summary>
    /// Constructor for Like expression with specified keyword, mainly for ilike
    /// </summary>
    /// <param name="isNegated"></param>
    /// <param name="keyword"></param>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <param name="escapeOption"></param>
    public LikeExpression(bool isNegated, string keyword, IValueExpression left, IValueExpression right, string escapeOption = "")
    {
        Keyword = keyword;
        IsNegated = isNegated;
        Left = left;
        Right = right;
        EscapeOption = escapeOption;

    }

    public string DefaultName => Left.DefaultName;

    public bool MightHaveQueries => Left.MightHaveQueries || Right.MightHaveQueries;

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        foreach (var lexeme in Left.GenerateTokensWithoutCte())
        {
            yield return lexeme;
        }
        yield return new Token(TokenType.Operator, IsNegated ? $"not {Keyword}" : Keyword);
        foreach (var lexeme in Right.GenerateTokensWithoutCte())
        {
            yield return lexeme;
        }
        if (!string.IsNullOrEmpty(EscapeOption))
        {
            yield return new Token(TokenType.Command, "escape");
            yield return new Token(TokenType.Literal, EscapeOption);
        }
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(Left.ToSqlWithoutCte());
        sb.Append(IsNegated ? $" not {Keyword} " : $" {Keyword} ");
        sb.Append(Right.ToSqlWithoutCte());
        if (!string.IsNullOrEmpty(EscapeOption))
        {
            sb.Append(" escape ");
            sb.Append(EscapeOption);
        }
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
