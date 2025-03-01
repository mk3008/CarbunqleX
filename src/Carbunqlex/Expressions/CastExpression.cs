using Carbunqlex.Lexing;

namespace Carbunqlex.Expressions;

/// <summary>
/// Represents a SQL CAST expression, which converts a value from one data type to another.
/// </summary>
public class CastExpression : IValueExpression
{
    public IValueExpression Expression { get; }
    public string TargetType { get; }

    public CastExpression(IValueExpression expression, string targetType)
    {
        Expression = expression;
        TargetType = targetType;
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => Expression.MightHaveQueries;

    public string ToSqlWithoutCte()
    {
        return $"cast({Expression.ToSqlWithoutCte()} as {TargetType})";
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.Command, "cast");
        yield return new Token(TokenType.OpenParen, "(");
        foreach (var lexeme in Expression.GenerateTokensWithoutCte())
        {
            yield return lexeme;
        }
        yield return new Token(TokenType.Command, "as");
        yield return new Token(TokenType.Identifier, TargetType);
        yield return new Token(TokenType.CloseParen, ")");
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return Expression.GetQueries();
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        return Expression.ExtractColumnExpressions();
    }
}
