using Carbunqlex.Clauses;

namespace Carbunqlex;

/// <summary>
/// Represents a SQL component that can generate SQL strings and lexemes.
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
    /// Generates the lexemes for the component, excluding the WITH clause.
    /// This is used to control the inclusion of common table expressions (CTEs) at the root level of the query.
    /// Note: SQL components do not include the WITH clause.
    /// </summary>
    /// <returns>The lexemes representing the component without the WITH clause.</returns>
    IEnumerable<Lexeme> GenerateLexemesWithoutCte();

    /// <summary>
    /// Retrieves the common table clauses (CTEs) associated with the component.
    /// This is used to collect CTEs from nested queries and include them in the root query's WITH clause.
    /// </summary>
    /// <returns>The common table clauses associated with the component.</returns>
    IEnumerable<CommonTableClause> GetCommonTableClauses();
}
