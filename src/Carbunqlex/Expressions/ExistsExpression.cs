using Carbunqlex.Lexing;
using System.Text;

namespace Carbunqlex.Expressions;

public class ExistsExpression : IValueExpression
{
    public ISelectQuery Query { get; set; }
    public bool IsNegated { get; set; }

    public ExistsExpression(bool isNegated, ISelectQuery query)
    {
        IsNegated = isNegated;
        Query = query;
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => true;

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.Operator, IsNegated ? "not exists" : "exists");
        yield return new Token(TokenType.OpenParen, "(");
        foreach (var lexeme in Query.GenerateTokensWithoutCte())
        {
            yield return lexeme;
        }
        yield return new Token(TokenType.CloseParen, ")");
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(IsNegated ? "not exists (" : "exists (");
        sb.Append(Query.ToSqlWithoutCte());
        sb.Append(")");
        return sb.ToString();
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        yield return Query;
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        return Query.ExtractColumnExpressions();
    }
}
