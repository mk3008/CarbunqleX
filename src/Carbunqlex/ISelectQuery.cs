﻿using Carbunqlex.Clauses;
using Carbunqlex.Expressions;
using Carbunqlex.Lexing;
using Carbunqlex.QuerySources;
using System.Diagnostics.CodeAnalysis;

namespace Carbunqlex;

public interface IQuery
{
    /// <summary>
    /// Generates the SQL string for the query.
    /// This can include the WITH clause if the query is at the root level.
    /// </summary>
    /// <returns>The SQL string representation of the query.</returns>
    string ToSql();

    /// <summary>
    /// Retrieves the parameters used in the query.
    /// </summary>
    /// <returns></returns>
    IDictionary<string, object?> GetParameters();

    bool TryGetSelectQuery([NotNullWhen(true)] out ISelectQuery? selectQuery);

    bool TryGetSelectClause([NotNullWhen(true)] out SelectClause? selectClause);

    bool TryGetWhereClause([NotNullWhen(true)] out WhereClause? whereClause);
}

public interface IQueryComponent : ISqlComponent, IQuery
{
    /// <summary>
    /// Generates the tokens for the query.
    /// This can include the WITH clause if the query is at the root level.
    /// </summary>
    /// <returns>The tokens representing the query.</returns>
    IEnumerable<Token> GenerateTokens();

    /// <summary>
    /// Retrieves the common table clauses (CTEs) associated with the component.
    /// This is used to collect CTEs from nested queries and include them in the root query's WITH clause.
    /// </summary>
    /// <returns>The common table clauses associated with the component.</returns>
    IEnumerable<CommonTableClause> GetCommonTableClauses();

    /// <summary>
    /// Adds a parameter to the query.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    ParameterExpression AddParameter(string name, object value);
}

/// <summary>
/// Represents a SQL query that can generate SQL strings and tokens, with or without CTEs.
/// </summary>
public interface ISelectQuery : IQueryComponent, IArgumentExpression
{
    /// <summary>
    /// Retrieves the select expressions.
    /// </summary>
    /// <returns></returns>
    IEnumerable<SelectExpression> GetSelectExpressions();

    /// <summary>
    /// Retrieves the datasources.
    /// </summary>
    /// <returns></returns>
    IEnumerable<DatasourceExpression> GetDatasources();

    void AddColumn(SelectExpression expr);

    void AddColumn(IValueExpression value, string alias);

    void RemoveColumn(SelectExpression expr);

    void AddJoin(JoinClause joinClause);
}

//public static class ISelectQueryExtensions
//{
//    public static ParameterExpression AddParameter(this ISelectQuery query, string name, object value)
//    {
//        var parameter = new ParameterExpression(name);
//        query.GetParameters().Add(name, value);
//        return parameter;
//    }
//}
