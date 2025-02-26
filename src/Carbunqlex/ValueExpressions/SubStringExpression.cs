using Carbunqlex.Lexing;

namespace Carbunqlex.ValueExpressions;

public class SubStringExpression : IValueExpression
{
    public IValueExpression Source { get; }
    public IValueExpression? From { get; }
    public IValueExpression? For { get; }

    public SubStringExpression(IValueExpression source, IValueExpression? from = null, IValueExpression? @for = null)
    {
        if (from is null && @for is null)
            throw new ArgumentException("At least one of 'From' or 'For' must be specified.");

        Source = source;
        From = from;
        For = @for;
    }

    public string ToSqlWithoutCte()
    {
        var from = From != null
            ? $" from {From.ToSqlWithoutCte()}"
            : string.Empty;
        var @for = For != null
            ? $" for {For.ToSqlWithoutCte()}"
            : string.Empty;
        return $"substring({Source.ToSqlWithoutCte()}{from}{@for})";
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>
        {
            new Token(TokenType.Command, "substring"),
            new Token(TokenType.OpenParen, "(")
        };

        tokens.AddRange(Source.GenerateTokensWithoutCte());

        if (From != null)
        {
            tokens.Add(new Token(TokenType.Command, "from"));
            tokens.AddRange(From.GenerateTokensWithoutCte());
        }

        if (For != null)
        {
            tokens.Add(new Token(TokenType.Command, "for"));
            tokens.AddRange(For.GenerateTokensWithoutCte());
        }

        tokens.Add(new Token(TokenType.CloseParen, ")"));

        return tokens;
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => false;

    public IEnumerable<ColumnExpression> ExtractColumnExpressions() => Enumerable.Empty<ColumnExpression>();

    public IEnumerable<ISelectQuery> GetQueries() => Enumerable.Empty<ISelectQuery>();
}

public class SubStringSimilarExpression : IValueExpression
{
    public IValueExpression Source { get; }
    public IValueExpression Pattern { get; }
    public IValueExpression Escape { get; }

    public SubStringSimilarExpression(IValueExpression source, IValueExpression pattern, IValueExpression escape)
    {
        Source = source;
        Pattern = pattern;
        Escape = escape;
    }

    public string ToSqlWithoutCte()
    {
        return $"substring({Source.ToSqlWithoutCte()} similar {Pattern.ToSqlWithoutCte()} escape {Escape.ToSqlWithoutCte()})";
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>
        {
            new Token(TokenType.Command, "substring"),
            new Token(TokenType.OpenParen, "(")
        };
        tokens.AddRange(Source.GenerateTokensWithoutCte());
        tokens.Add(new Token(TokenType.Command, "similar"));
        tokens.AddRange(Pattern.GenerateTokensWithoutCte());
        tokens.Add(new Token(TokenType.Command, "escape"));
        tokens.AddRange(Escape.GenerateTokensWithoutCte());
        tokens.Add(new Token(TokenType.CloseParen, ")"));
        return tokens;
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => false;

    public IEnumerable<ColumnExpression> ExtractColumnExpressions() => Enumerable.Empty<ColumnExpression>();

    public IEnumerable<ISelectQuery> GetQueries() => Enumerable.Empty<ISelectQuery>();
}
