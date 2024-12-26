using System.Text;

namespace Carbunqlex.Clauses;

public class ColumnAliasClause : IColumnAliasClause
{
    public List<string> ColumnAliases { get; set; } = new();

    public ColumnAliasClause(IEnumerable<string> aliases)
    {
        ColumnAliases = aliases.ToList();
    }

    public IEnumerable<string> GetColumnNames() => ColumnAliases;

    public string ToSqlWithoutCte()
    {
        if (!ColumnAliases.Any())
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.Append("(");
        sb.Append(string.Join(", ", ColumnAliases));
        sb.Append(")");
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        if (!ColumnAliases.Any())
        {
            return Enumerable.Empty<Lexeme>();
        }

        var lexemes = new List<Lexeme>(ColumnAliases.Count * 2 + 2)
        {
            new Lexeme(LexType.OpenParen, "(")
        };

        for (int i = 0; i < ColumnAliases.Count; i++)
        {
            lexemes.Add(new Lexeme(LexType.Identifier, ColumnAliases[i]));
            if (i < ColumnAliases.Count - 1)
            {
                lexemes.Add(new Lexeme(LexType.Comma, ","));
            }
        }

        lexemes.Add(new Lexeme(LexType.CloseParen, ")"));
        return lexemes;
    }

    public IEnumerable<IQuery> GetQueries()
    {
        // ColumnAliases does not directly use queries, so return an empty list
        return Enumerable.Empty<IQuery>();
    }
}
