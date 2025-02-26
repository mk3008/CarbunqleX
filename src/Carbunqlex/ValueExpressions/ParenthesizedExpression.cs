using Carbunqlex.Lexing;
using System.Text;

namespace Carbunqlex.ValueExpressions;

public class ParenthesizedExpression : IValueExpression
{
    public IValueExpression InnerExpression { get; set; }

    public ParenthesizedExpression(IValueExpression innerExpression)
    {
        InnerExpression = innerExpression;
    }

    public string DefaultName => InnerExpression.DefaultName;

    public bool MightHaveQueries => InnerExpression.MightHaveQueries;

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.OpenParen, "(");
        foreach (var lexeme in InnerExpression.GenerateTokensWithoutCte())
        {
            yield return lexeme;
        }
        yield return new Token(TokenType.CloseParen, ")");
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append("(");
        sb.Append(InnerExpression.ToSqlWithoutCte());
        sb.Append(")");
        return sb.ToString();
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        if (InnerExpression.MightHaveQueries)
        {
            return InnerExpression.GetQueries();
        }
        return Enumerable.Empty<ISelectQuery>();
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        return InnerExpression.ExtractColumnExpressions();
    }
}
