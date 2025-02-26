using Carbunqlex.Lexing;
using System.Text;

namespace Carbunqlex.Clauses;

public class CommonTableClause : ISqlComponent
{
    public bool IsRecursive { get; set; }
    public string Alias { get; set; }
    public ISelectQuery Query { get; }
    public ColumnAliasClause? ColumnAliasClause { get; }
    public bool? IsMaterialized { get; }

    public CommonTableClause(ISelectQuery query, string alias, ColumnAliasClause? columnAliases = null, bool? isMaterialized = null, bool isRecursive = false)
    {
        Query = query;
        Alias = alias;
        ColumnAliasClause = columnAliases;
        IsMaterialized = isMaterialized;
        IsRecursive = isRecursive;
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(Alias);

        if (ColumnAliasClause != null && ColumnAliasClause.ColumnAliases.Any())
        {
            sb.Append(ColumnAliasClause.ToSqlWithoutCte());
        }

        sb.Append(" as");

        if (IsMaterialized != null)
        {
            sb.Append(IsMaterialized.Value ? " materialized" : " not materialized");
        }

        sb.Append(" (");
        sb.Append(Query.ToSqlWithoutCte());
        sb.Append(")");
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>
        {
            new Token(TokenType.Identifier, Alias)
        };

        if (ColumnAliasClause != null && ColumnAliasClause.ColumnAliases.Any())
        {
            tokens.AddRange(ColumnAliasClause.GenerateTokensWithoutCte());
        }

        tokens.Add(Token.AsKeyword);

        if (IsMaterialized != null)
        {
            if (IsMaterialized.Value)
            {
                tokens.Add(new Token(TokenType.Command, "materialized"));
            }
            else
            {
                tokens.Add(new Token(TokenType.Command, "not materialized"));
            }
        }

        tokens.Add(new Token(TokenType.OpenParen, "("));

        tokens.AddRange(Query.GenerateTokensWithoutCte());

        tokens.Add(new Token(TokenType.CloseParen, ")"));

        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return Query.GetQueries().Union([Query]);
    }
}
