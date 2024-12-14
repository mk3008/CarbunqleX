using Carbunqlex.DatasourceExpressions;
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
    public IQuery Query { get; }
    public ColumnAliases? ColumnAliases { get; }
    public Materialization Materialization { get; }

    public CommonTableClause(IQuery query, string alias, ColumnAliases? columnAliases = null, Materialization materialization = Materialization.None, bool isRecursive = false)
    {
        Query = query;
        Alias = alias;
        ColumnAliases = columnAliases;
        Materialization = materialization;
        IsRecursive = isRecursive;
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(Alias);

        if (ColumnAliases != null && ColumnAliases.Aliases.Any())
        {
            sb.Append(ColumnAliases.ToSqlWithoutCte());
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

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        var lexemes = new List<Lexeme>
        {
            new Lexeme(LexType.Identifier, Alias)
        };

        if (ColumnAliases != null && ColumnAliases.Aliases.Any())
        {
            lexemes.AddRange(ColumnAliases.GenerateLexemesWithoutCte());
        }

        lexemes.Add(Lexeme.AsKeyword);

        var materializationSql = Materialization.ToSqlString();
        if (!string.IsNullOrEmpty(materializationSql))
        {
            lexemes.Add(new Lexeme(LexType.Keyword, materializationSql));
        }

        lexemes.Add(new Lexeme(LexType.OpenParen, "("));

        lexemes.AddRange(Query.GenerateLexemesWithoutCte());

        lexemes.Add(new Lexeme(LexType.CloseParen, ")"));

        return lexemes;
    }

    public IEnumerable<IQuery> GetQueries()
    {
        var queries = new List<IQuery>();
        queries.AddRange(Query.GetQueries());
        queries.Add(Query);
        return queries;
    }
}
