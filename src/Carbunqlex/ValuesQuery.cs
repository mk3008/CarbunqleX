﻿using Carbunqlex.Clauses;
using Carbunqlex.Expressions;
using Carbunqlex.Lexing;
using Carbunqlex.QuerySources;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Carbunqlex;

public class ValuesQuery : ISelectQuery
{
    public readonly List<ValuesRow> Rows;
    private int? columnCount;
    public bool MightHaveQueries => false;

    public ValuesQuery(List<ValuesRow> rows)
    {
        Rows = rows;
        if (rows.Count != 0)
        {
            columnCount = rows[0].Columns.Count;
        }
    }

    public ValuesQuery()
    {
        Rows = new List<ValuesRow>();
    }

    public void AddRow(IEnumerable<IValueExpression> columns)
    {
        var row = columns.ToList();
        if (columnCount == null)
        {
            columnCount = row.Count;
        }
        else if (row.Count != columnCount)
        {
            throw new ArgumentException($"All rows must have the same number of columns. Expected {columnCount}, but got {row.Count}.");
        }

        Rows.Add(new ValuesRow(row));
    }

    public string ToSql()
    {
        var sb = new StringBuilder();

        var withSql = new WithClause(GetCommonTableClauses()).ToSql();
        if (!string.IsNullOrEmpty(withSql))
        {
            sb.Append(withSql).Append(" ");
        }

        sb.Append("values ");
        for (int i = 0; i < Rows.Count; i++)
        {
            sb.Append(Rows[i].ToSqlWithoutCte());
            if (i < Rows.Count - 1)
            {
                sb.Append(", ");
            }
        }
        return sb.ToString();
    }

    public string ToSqlWithoutCte()
    {
        return ToSql();
    }

    public IEnumerable<Token> GenerateTokens()
    {
        int capacity = Rows.Sum(row => row.Capacity) + (Rows.Count - 1) + 1;

        var tokens = new List<Token>(capacity) { new Token(TokenType.Command, "VALUES") };

        for (int i = 0; i < Rows.Count; i++)
        {
            tokens.AddRange(Rows[i].GenerateTokensWithoutCte());
            if (i < Rows.Count - 1)
            {
                tokens.Add(new Token(TokenType.Comma, ","));
            }
        }
        return tokens;
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        return GenerateTokens();
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        var commonTableClauses = new List<CommonTableClause>();

        foreach (var row in Rows)
        {
            commonTableClauses.AddRange(row.GetQueries().SelectMany(x => x.GetCommonTableClauses()));
        }

        return commonTableClauses
            .GroupBy(cte => cte.Alias)
            .Select(group => group.First())
            .OrderByDescending(cte => cte.IsRecursive);
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery>();

        queries.Add(this);
        foreach (var row in Rows)
        {
            queries.AddRange(row.GetQueries());
        }

        return queries;
    }

    public Dictionary<string, object?> Parameters { get; } = new();

    public IDictionary<string, object?> GetParameters()
    {
        var parameters = new Dictionary<string, object?>();

        // Add own parameters first
        foreach (var parameter in Parameters)
        {
            parameters[parameter.Key] = parameter.Value;
        }

        // Add internal parameters, excluding duplicates
        foreach (var parameter in GetQueries().Where(q => q != this).SelectMany(q => q.GetParameters()))
        {
            if (!parameters.ContainsKey(parameter.Key))
            {
                parameters[parameter.Key] = parameter.Value;
            }
        }

        return parameters;
    }

    public IEnumerable<SelectExpression> GetSelectExpressions()
    {
        return Enumerable.Empty<SelectExpression>();
    }

    public IEnumerable<DatasourceExpression> GetDatasources()
    {
        return Enumerable.Empty<DatasourceExpression>();
    }
    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        return Rows.SelectMany(row => row.Columns.SelectMany(column => column.ExtractColumnExpressions()));
    }

    public bool TryGetWhereClause([NotNullWhen(true)] out WhereClause? whereClause)
    {
        whereClause = null;
        return false;
    }

    public ParameterExpression AddParameter(string name, object value)
    {
        var parameter = new ParameterExpression(name);
        Parameters[name] = value;
        return parameter;
    }

    public void AddColumn(SelectExpression expr)
    {
        throw new NotSupportedException("ValuesQuery does not support adding columns.");
    }

    public void RemoveColumn(SelectExpression expr)
    {
        throw new NotSupportedException("ValuesQuery does not support removing columns.");
    }

    public void AddColumn(IValueExpression value, string alias)
    {
        throw new NotSupportedException("ValuesQuery does not support adding columns.");
    }

    public void AddJoin(JoinClause joinClause)
    {
        throw new NotSupportedException("ValuesQuery does not support adding joins.");
    }

    public bool TryGetSelectQuery([NotNullWhen(true)] out ISelectQuery? selectQuery)
    {
        selectQuery = this;
        return true;
    }

    public bool TryGetSelectClause([NotNullWhen(true)] out SelectClause? selectClause)
    {
        selectClause = null;
        return false;
    }
}
