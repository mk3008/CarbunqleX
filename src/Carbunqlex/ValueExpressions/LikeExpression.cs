using System.Text;

namespace Carbunqlex.ValueExpressions;

public class LikeExpression : IValueExpression
{
    public IValueExpression Left { get; set; }
    public IValueExpression Right { get; set; }
    public bool IsNegated { get; set; }
    public char? EscapeCharacter { get; set; }

    public LikeExpression(bool isNegated, IValueExpression left, IValueExpression right, char? escapeCharacter = null)
    {
        IsNegated = isNegated;
        Left = left;
        Right = right;
        EscapeCharacter = escapeCharacter;
    }

    public string DefaultName => Left.DefaultName;

    public bool MightHaveQueries => Left.MightHaveQueries || Right.MightHaveQueries;

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        foreach (var lexeme in Left.GenerateTokensWithoutCte())
        {
            yield return lexeme;
        }
        yield return new Token(TokenType.Operator, IsNegated ? "not like" : "like");
        foreach (var lexeme in Right.GenerateTokensWithoutCte())
        {
            yield return lexeme;
        }
        if (EscapeCharacter.HasValue)
        {
            yield return new Token(TokenType.Keyword, "escape");
            yield return new Token(TokenType.Value, $"'{EscapeCharacter}'");
        }
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(Left.ToSqlWithoutCte());
        sb.Append(IsNegated ? " not like " : " like ");
        sb.Append(Right.ToSqlWithoutCte());
        if (EscapeCharacter.HasValue)
        {
            sb.Append(" escape '");
            sb.Append(EscapeCharacter);
            sb.Append("'");
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
