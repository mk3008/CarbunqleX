namespace Carbunqlex.ValueExpressions;

public class NormalizeExpression : IValueExpression
{
    public IValueExpression OriginalText { get; set; }
    public string Form { get; set; }

    public NormalizeExpression(IValueExpression originalText, string form = "NFC")
    {
        OriginalText = originalText;
        Form = form;
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => false;

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        yield break;
    }

    public string ToSqlWithoutCte()
    {
        if (Form.Equals("NFC", StringComparison.OrdinalIgnoreCase))
        {
            return $"normalize({OriginalText.ToSqlWithoutCte()})";
        }
        return $"normalize({OriginalText.ToSqlWithoutCte()}, {Form})";
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>
        {
            new Token(TokenType.Command, "normalize"),
            new Token(TokenType.OpenParen, "("),
        };
        foreach (var lexeme in OriginalText.GenerateTokensWithoutCte())
        {
            tokens.Add(lexeme);
        }
        if (!Form.Equals("NFC", StringComparison.OrdinalIgnoreCase))
        {
            tokens.Add(new Token(TokenType.Comma, ","));
            tokens.Add(new Token(TokenType.Command, Form));
        }
        tokens.Add(new Token(TokenType.CloseParen, ")"));
        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        yield break;
    }
}
