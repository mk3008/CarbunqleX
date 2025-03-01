using Carbunqlex.Lexing;
using Carbunqlex.QuerySources;

namespace Carbunqlex.Clauses;

public class DeleteClause : ISqlComponent
{
    public TableSource TableSource { get; set; }

    public string Alias { get; set; }

    public DeleteClause(TableSource tableSource, string alias)
    {
        TableSource = tableSource;
        Alias = string.IsNullOrEmpty(alias)
            ? tableSource.DefaultName
            : alias;
    }

    public DeleteClause(TableSource tableSource)
    {
        TableSource = tableSource;
        Alias = tableSource.DefaultName;
    }

    public bool HasAliasToken => !string.IsNullOrWhiteSpace(Alias) && Alias != TableSource.DefaultName;

    public string ToSqlWithoutCte()
    {
        if (HasAliasToken)
        {
            return $"delete from {TableSource.ToSqlWithoutCte()} as {Alias}";
        }
        return $"delete from {TableSource.ToSqlWithoutCte()}";
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.Command, "delete from");
        yield return new Token(TokenType.Identifier, TableSource.TableFullName);
        if (HasAliasToken)
        {
            yield return new Token(TokenType.Identifier, Alias);
        }
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        yield break;
    }
}
