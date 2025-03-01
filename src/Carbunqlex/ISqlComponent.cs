using Carbunqlex.Lexing;

namespace Carbunqlex;

/// <summary>
/// Represents a SQL component that can generate SQL strings and tokens.
/// </summary>
public interface ISqlComponent
{
    /// <summary>
    /// Generates the SQL string for the component, excluding the WITH clause.
    /// This is used to control the inclusion of common table expressions (CTEs) at the root level of the query.
    /// Note: SQL components do not include the WITH clause.
    /// </summary>
    /// <returns>The SQL string representation of the component without the WITH clause.</returns>
    string ToSqlWithoutCte();

    /// <summary>
    /// Generates the tokens for the component, excluding the WITH clause.
    /// This is used to control the inclusion of common table expressions (CTEs) at the root level of the query.
    /// Note: SQL components do not include the WITH clause.
    /// </summary>
    /// <returns>The tokens representing the component without the WITH clause.</returns>
    IEnumerable<Token> GenerateTokensWithoutCte();

    /// <summary>
    /// Retrieves the queries used internally, including itself.
    /// </summary>
    /// <returns>An enumerable of queries.</returns>
    IEnumerable<ISelectQuery> GetQueries();
}
