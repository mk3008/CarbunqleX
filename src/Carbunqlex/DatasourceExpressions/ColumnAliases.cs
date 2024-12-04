namespace Carbunqlex.DatasourceExpressions;

public class ColumnAliases : ISqlComponent
{
    public List<string> Aliases { get; set; } = new();

    public ColumnAliases(IEnumerable<string> aliases)
    {
        Aliases = aliases.ToList();
    }

    public string ToSql()
    {
        return Aliases.Any() ? $"({string.Join(", ", Aliases)})" : string.Empty;
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        if (!Aliases.Any())
        {
            return Enumerable.Empty<Lexeme>();
        }

        var lexemes = new List<Lexeme>(Aliases.Count * 2 + 2)
        {
            new Lexeme(LexType.OpenParen, "(")
        };

        for (int i = 0; i < Aliases.Count; i++)
        {
            lexemes.Add(new Lexeme(LexType.Identifier, Aliases[i]));
            if (i < Aliases.Count - 1)
            {
                lexemes.Add(new Lexeme(LexType.Comma, ","));
            }
        }

        lexemes.Add(new Lexeme(LexType.CloseParen, ")"));
        return lexemes;
    }
}