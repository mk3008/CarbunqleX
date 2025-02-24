namespace Carbunqlex.ValueExpressions;

public class OverlayExpression : IValueExpression
{
    public IValueExpression OriginalText { get; set; }
    public IValueExpression NewSubstring { get; set; }
    public IValueExpression Start { get; set; }
    public IValueExpression? Count { get; set; }

    public OverlayExpression(IValueExpression originalText, IValueExpression newSubstring, IValueExpression start, IValueExpression? count = null)
    {
        OriginalText = originalText;
        NewSubstring = newSubstring;
        Start = start;
        Count = count;
    }

    public string ToSqlWithoutCte()
    {
        if (Count == null)
        {
            return $"overlay({OriginalText.ToSqlWithoutCte()} placing {NewSubstring.ToSqlWithoutCte()} from {Start.ToSqlWithoutCte()})";
        }
        else
        {
            return $"overlay({OriginalText.ToSqlWithoutCte()} placing {NewSubstring.ToSqlWithoutCte()} from {Start.ToSqlWithoutCte()} for {Count.ToSqlWithoutCte()})";
        }
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>
        {
            new Token(TokenType.Command, "overlay"),
            new Token(TokenType.OpenParen, "("),
        };
        foreach (var lexeme in OriginalText.GenerateTokensWithoutCte())
        {
            tokens.Add(lexeme);
        }
        tokens.Add(new Token(TokenType.Command, "placing"));
        foreach (var lexeme in NewSubstring.GenerateTokensWithoutCte())
        {
            tokens.Add(lexeme);
        }
        tokens.Add(new Token(TokenType.Command, "from"));
        foreach (var lexeme in Start.GenerateTokensWithoutCte())
        {
            tokens.Add(lexeme);
        }
        if (Count != null)
        {
            tokens.Add(new Token(TokenType.Command, "for"));
            foreach (var lexeme in Count.GenerateTokensWithoutCte())
            {
                tokens.Add(lexeme);
            }
        }
        tokens.Add(new Token(TokenType.CloseParen, ")"));
        return tokens;
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => false;

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        yield break;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        yield break;
    }
}
