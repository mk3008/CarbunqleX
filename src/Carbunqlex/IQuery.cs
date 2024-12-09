namespace Carbunqlex;

/// <summary>
/// Represents a SQL query that can generate SQL strings and lexemes, with or without CTEs.
/// </summary>
public interface IQuery : ISqlComponent
{
    /// <summary>
    /// Generates the SQL string for the query.
    /// This can include the WITH clause if the query is at the root level.
    /// </summary>
    /// <returns>The SQL string representation of the query.</returns>
    string ToSql();

    /// <summary>
    /// Generates the lexemes for the query.
    /// This can include the WITH clause if the query is at the root level.
    /// </summary>
    /// <returns>The lexemes representing the query.</returns>
    IEnumerable<Lexeme> GenerateLexemes();
}
