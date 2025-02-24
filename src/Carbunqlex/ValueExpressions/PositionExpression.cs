namespace Carbunqlex.ValueExpressions;

public class PositionExpression : IValueExpression
{
    public IValueExpression SubString { get; set; }

    public IValueExpression SourceString { get; set; }

    public PositionExpression(IValueExpression subString, IValueExpression sourceString)
    {
        SubString = subString;
        SourceString = sourceString;
    }

    public string ToSqlWithoutCte()
    {
        return $"position({SubString.ToSqlWithoutCte()} in {SourceString.ToSqlWithoutCte()})";
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>
        {
            new Token(TokenType.Command, "position"),
            new Token(TokenType.OpenParen, "("),
        };
        foreach (var lexeme in SubString.GenerateTokensWithoutCte())
        {
            tokens.Add(lexeme);
        }
        tokens.Add(new Token(TokenType.Command, "in"));
        foreach (var lexeme in SourceString.GenerateTokensWithoutCte())
        {
            tokens.Add(lexeme);
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
