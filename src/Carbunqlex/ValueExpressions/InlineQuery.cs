namespace Carbunqlex.ValueExpressions;

public class InlineQuery : IValueExpression
{
    public IQuery Query { get; }
    public string DefaultName => string.Empty;

    public InlineQuery(IQuery query)
    {
        Query = query;
    }

    public string ToSql()
    {
        return $"({Query.ToSql()})";
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        var lexemes = new List<Lexeme>
        {
            new Lexeme(LexType.OpenParen, "(", "inline_query")
        };

        lexemes.AddRange(Query.GetLexemes());

        lexemes.Add(new Lexeme(LexType.CloseParen, ")", "inline_query"));

        return lexemes;
    }
}
