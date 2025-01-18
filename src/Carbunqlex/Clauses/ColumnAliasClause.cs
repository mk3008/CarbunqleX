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

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        if (!ColumnAliases.Any())
        {
            return Enumerable.Empty<Token>();
        }

        var tokens = new List<Token>(ColumnAliases.Count * 2 + 2)
        {
            new Token(TokenType.OpenParen, "(")
        };

        for (int i = 0; i < ColumnAliases.Count; i++)
        {
            tokens.Add(new Token(TokenType.Identifier, ColumnAliases[i]));
            if (i < ColumnAliases.Count - 1)
            {
                tokens.Add(new Token(TokenType.Comma, ","));
            }
        }

        tokens.Add(new Token(TokenType.CloseParen, ")"));
        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        // ColumnAliases does not directly use queries, so return an empty list
        return Enumerable.Empty<ISelectQuery>();
    }
}
