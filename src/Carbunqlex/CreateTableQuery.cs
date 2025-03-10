﻿using Carbunqlex.Clauses;
using Carbunqlex.Expressions;
using Carbunqlex.Lexing;
using Carbunqlex.QuerySources;
using System.Diagnostics.CodeAnalysis;

namespace Carbunqlex;

public class CreateTableAsQuery : IQueryComponent
{
    public TableSource TableSource { get; set; }
    public ISelectQuery SelectQuery { get; set; }
    public bool IsTemporary { get; set; }
    public Dictionary<string, object?> Parameters { get; } = new();
    public CreateTableAsQuery(TableSource tableSource, ISelectQuery selectQuery, bool isTemporary)
    {
        TableSource = tableSource;
        SelectQuery = selectQuery;
        IsTemporary = isTemporary;
    }

    public string ToSql()
    {
        var command = IsTemporary ? "create temporary table" : "create table";
        return $"{command} {TableSource.TableFullName} as {SelectQuery.ToSql()}";
    }

    public IEnumerable<Token> GenerateTokens()
    {
        var command = IsTemporary ? "create temporary table" : "create table";
        return new List<Token>
        {
            new Token(TokenType.Command , command),
            new Token(TokenType.Identifier, TableSource.TableFullName),
            new Token(TokenType.Command, "as"),
        }.Concat(SelectQuery.GenerateTokens());
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        yield break;
    }

    public IDictionary<string, object?> GetParameters()
    {
        return Parameters;
    }

    public ParameterExpression AddParameter(string name, object value)
    {
        Parameters[name] = value;
        return new ParameterExpression(name);
    }

    public string ToSqlWithoutCte()
    {
        throw new InvalidCastException("Can't convert to without CTE query.");
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        throw new InvalidCastException("Can't convert to without CTE query.");
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        yield break;
    }

    public bool TryGetSelectQuery([NotNullWhen(true)] out ISelectQuery? selectQuery)
    {
        selectQuery = SelectQuery;
        return true;
    }

    public bool TryGetWhereClause([NotNullWhen(true)] out WhereClause? whereClause)
    {
        return SelectQuery.TryGetWhereClause(out whereClause);
    }

    public bool TryGetSelectClause([NotNullWhen(true)] out SelectClause? selectClause)
    {
        return SelectQuery.TryGetSelectClause(out selectClause);
    }
}
