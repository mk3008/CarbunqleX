﻿using Carbunqlex.Clauses;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Carbunqlex.DatasourceExpressions;

public class SubQuerySource : IDatasource
{
    public ISelectQuery Query { get; set; }
    public string Alias { get; set; }
    public IColumnAliasClause ColumnAliasClause { get; set; }
    public string TableFullName => string.Empty;
    public SubQuerySource(ISelectQuery query, string alias)
    {
        Query = query;
        Alias = alias;
        ColumnAliasClause = EmptyColumnAliasClause.Instance;
    }

    public SubQuerySource(ISelectQuery query, string alias, IEnumerable<string> columnAliases)
    {
        Query = query;
        Alias = alias;
        ColumnAliasClause = new ColumnAliasClause(columnAliases);
    }

    public string ToSqlWithoutCte()
    {
        if (string.IsNullOrWhiteSpace(Alias))
        {
            throw new ArgumentException("Alias is required for a function source.", nameof(Alias));
        }
        var sb = new StringBuilder();
        sb.Append("(");
        sb.Append(Query.ToSqlWithoutCte());
        sb.Append(") as ");
        sb.Append(Alias);
        sb.Append(ColumnAliasClause.ToSqlWithoutCte());
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>
        {
            new Token(TokenType.OpenParen, "(")
        };
        tokens.AddRange(Query.GenerateTokensWithoutCte());
        tokens.Add(new Token(TokenType.CloseParen, ")"));
        tokens.Add(new Token(TokenType.Keyword, "as"));
        tokens.Add(new Token(TokenType.Identifier, Alias));
        tokens.AddRange(ColumnAliasClause.GenerateTokensWithoutCte());
        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery> { Query };
        queries.AddRange(Query.GetQueries());
        return queries;
    }

    public IEnumerable<string> GetSelectableColumns()
    {
        if (ColumnAliasClause.GetColumnNames().Any())
        {
            return ColumnAliasClause.GetColumnNames();
        }
        else
        {
            return Query.GetSelectExpressions().Select(column => column.Alias);
        }
    }

    public bool TryGetSubQuery([NotNullWhen(true)] out ISelectQuery subQuery)
    {
        subQuery = Query;
        return true;
    }

    public bool TryGetTableName([NotNullWhen(true)] out string? tableFullName)
    {
        tableFullName = null;
        return false;
    }

    public bool TryGetUnionQuerySource([NotNullWhen(true)] out UnionQuerySource? unionQuerySource)
    {
        unionQuerySource = null;
        return false;
    }
}
