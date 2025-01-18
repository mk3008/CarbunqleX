using System.Text;

namespace Carbunqlex.Clauses;

public enum Materialization
{
    None,
    Materialized,
    NotMaterialized
}

internal static class MaterializationExtensions
{
    /// <summary>
    /// Converts the Materialization enum to its corresponding SQL string representation.
    /// This method uses a switch statement for optimal performance.
    /// </summary>
    /// <param name="materialization">The Materialization enum value.</param>
    /// <returns>The SQL string representation of the Materialization.</returns>
    public static string ToSqlString(this Materialization materialization)
    {
        return materialization switch
        {
            Materialization.Materialized => "materialized",
            Materialization.NotMaterialized => "not materialized",
            Materialization.None => string.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(materialization), materialization, null)
        };
    }
}

public class CommonTableClause : ISqlComponent
{
    public bool IsRecursive { get; set; }
    public string Alias { get; set; }
    public ISelectQuery Query { get; }
    public ColumnAliasClause? ColumnAliasClause { get; }
    public Materialization Materialization { get; }

    public CommonTableClause(ISelectQuery query, string alias, ColumnAliasClause? columnAliases = null, Materialization materialization = Materialization.None, bool isRecursive = false)
    {
        Query = query;
        Alias = alias;
        ColumnAliasClause = columnAliases;
        Materialization = materialization;
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

        sb.Append(" as ");

        var materializationSql = Materialization.ToSqlString();
        if (!string.IsNullOrEmpty(materializationSql))
        {
            sb.Append(materializationSql).Append(" ");
        }

        sb.Append("(");
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

        var materializationSql = Materialization.ToSqlString();
        if (!string.IsNullOrEmpty(materializationSql))
        {
            tokens.Add(new Token(TokenType.Keyword, materializationSql));
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
