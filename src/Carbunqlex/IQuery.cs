using Carbunqlex.Clauses;
using Carbunqlex.DatasourceExpressions;
using Carbunqlex.ValueExpressions;

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

    /// <summary>
    /// Retrieves the common table clauses (CTEs) associated with the component.
    /// This is used to collect CTEs from nested queries and include them in the root query's WITH clause.
    /// </summary>
    /// <returns>The common table clauses associated with the component.</returns>
    IEnumerable<CommonTableClause> GetCommonTableClauses();

    /// <summary>
    /// Retrieves the parameters used in the query.
    /// </summary>
    /// <returns></returns>
    IDictionary<string, object?> GetParameters();

    /// <summary>
    /// Retrieves the select expressions.
    /// </summary>
    /// <returns></returns>
    IEnumerable<SelectExpression> GetSelectExpressions();

    /// <summary>
    /// Retrieves the datasources.
    /// </summary>
    /// <returns></returns>
    IEnumerable<IDatasource> GetDatasources();

    /// <summary>
    /// Retrieves the column expressions.
    /// </summary>
    /// <returns></returns>
    IEnumerable<ColumnExpression> ExtractColumnExpressions();
}
