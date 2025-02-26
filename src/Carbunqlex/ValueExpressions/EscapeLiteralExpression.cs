using Carbunqlex.Lexing;

namespace Carbunqlex.ValueExpressions;

public class EscapeLiteralExpression : IValueExpression
{
    public string EscapedLiteral { get; set; }

    public string EscapeOption { get; set; }

    public EscapeLiteralExpression(string escapedLiteral)
    {
        if (string.IsNullOrWhiteSpace(escapedLiteral))
        {
            throw new ArgumentException("Value cannot be null or empty.", nameof(escapedLiteral));
        }
        EscapedLiteral = escapedLiteral;
        EscapeOption = string.Empty;
    }

    public EscapeLiteralExpression(string escapedLiteral, string escapeOption)
    {
        if (string.IsNullOrWhiteSpace(escapedLiteral))
        {
            throw new ArgumentException("Value cannot be null or empty.", nameof(escapedLiteral));
        }
        EscapedLiteral = escapedLiteral;
        EscapeOption = escapeOption;
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => false;

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.Literal, EscapedLiteral.ToString()!);
    }

    public string ToSqlWithoutCte()
    {
        if (string.IsNullOrWhiteSpace(EscapeOption))
        {
            return $"{EscapedLiteral}";
        }
        return $"{EscapedLiteral} uescape {EscapeOption}";
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        // ConstantExpression does not directly use queries, so return an empty list
        return Enumerable.Empty<ISelectQuery>();
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        // ConstantExpression does not have columns, so return an empty list
        return Enumerable.Empty<ColumnExpression>();
    }
}
