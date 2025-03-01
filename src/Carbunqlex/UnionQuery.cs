using Carbunqlex.Clauses;
using Carbunqlex.Expressions;
using Carbunqlex.Lexing;
using Carbunqlex.QuerySources;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Carbunqlex;

public class UnionQuery : ISelectQuery
{
    public ISelectQuery Left { get; }
    public ISelectQuery Right { get; }
    public string UnionType { get; }
    public bool MightHaveQueries => true;

    public UnionQuery(string unionType, ISelectQuery left, ISelectQuery right)
    {
        Left = left;
        Right = right;
        UnionType = unionType;
    }

    /// <summary>
    /// Generates the SQL string for the union query, including the WITH clause if necessary.
    /// If there are duplicate CTEs, the one defined in the Left query takes precedence.
    /// </summary>
    /// <returns>The SQL string representation of the union query.</returns>
    public string ToSql()
    {
        var sb = new StringBuilder();

        // Generate merged WITH clause
        var withClause = new WithClause(GetCommonTableClauses()).ToSql();
        if (!string.IsNullOrEmpty(withClause))
        {
            sb.Append(withClause);
            sb.Append(" ");
        }

        // Combine Left and Right queries, excluding the WITH clause
        sb.Append(Left.ToSqlWithoutCte());
        sb.Append(" ");
        sb.Append(UnionType);
        sb.Append(" ");
        sb.Append(Right.ToSqlWithoutCte());

        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokens()
    {
        var leftTokens = Left.GenerateTokens().ToList();
        var rightTokens = Right.GenerateTokens().ToList();

        // Initial capacity is set to accommodate the tokens from Left, Right, and the UnionType keyword.
        var tokens = new List<Token>(leftTokens.Count + rightTokens.Count + 1);
        tokens.AddRange(leftTokens);
        tokens.Add(new Token(TokenType.Command, UnionType));
        tokens.AddRange(rightTokens);
        return tokens;
    }

    public string ToSqlWithoutCte()
    {
        return $"{Left.ToSqlWithoutCte()} {UnionType} {Right.ToSqlWithoutCte()}";
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var leftTokens = Left.GenerateTokensWithoutCte().ToList();
        var rightTokens = Right.GenerateTokensWithoutCte().ToList();

        // Initial capacity is set to accommodate the tokens from Left, Right, and the UnionType keyword.
        var tokens = new List<Token>(leftTokens.Count + rightTokens.Count + 1);
        tokens.AddRange(leftTokens);
        tokens.Add(new Token(TokenType.Command, UnionType));
        tokens.AddRange(rightTokens);
        return tokens;
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        var queries = Left.GetCommonTableClauses().Union(Right.GetCommonTableClauses());

        var commonTables = new List<(CommonTableClause Cte, int Index)>();
        commonTables.AddRange(queries.Select((cte, index) => (cte, index + commonTables.Count)));

        return commonTables
            .GroupBy(ct => ct.Cte.Alias)
            .Select(group => group.First())
            .OrderByDescending(cte => cte.Cte.IsRecursive)
            .ThenBy(ct => ct.Index)
            .Select(ct => ct.Cte);
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery>
        {
            this
        };
        queries.AddRange(Left.GetQueries());
        queries.AddRange(Right.GetQueries());

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
        // Since filtering cannot be applied to union queries, return an empty list
        return Enumerable.Empty<SelectExpression>();
    }

    internal IEnumerable<ISelectQuery> GetUnionQueryComponents()
    {
        if (Left is UnionQuery leftQuery)
        {
            foreach (var query in leftQuery.GetUnionQueryComponents())
            {
                yield return query;
            }
        }
        else
        {
            yield return Left;
        }
        if (Right is UnionQuery rightQuery)
        {
            foreach (var query in rightQuery.GetUnionQueryComponents())
            {
                yield return query;
            }
        }
        else
        {
            yield return Right;
        }
    }

    public IEnumerable<DatasourceExpression> GetDatasources()
    {
        var components = GetUnionQueryComponents().Select(static (query, index) => new { Index = index, Datasource = query }).ToList();
        var columns = Left.GetSelectExpressions().Select(x => x.Alias).ToList();

        foreach (var component in components)
        {
            yield return new DatasourceExpression(new UnionQuerySource(component.Datasource), component.Index.ToString(), new ColumnAliasClause(columns));
        }
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        return Left.ExtractColumnExpressions().Union(Right.ExtractColumnExpressions());
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
        throw new NotSupportedException("Columns cannot be added to a union query.");
    }

    public void RemoveColumn(SelectExpression expr)
    {
        throw new NotSupportedException("Columns cannot be removed from a union query.");
    }

    public void AddColumn(IValueExpression value, string alias)
    {
        throw new NotSupportedException("Columns cannot be added to a union query.");
    }

    public void AddJoin(JoinClause joinClause)
    {
        throw new NotSupportedException("Joins cannot be added to a union query.");
    }

    public bool TryGetSelectQuery([NotNullWhen(true)] out ISelectQuery? selectQuery)
    {
        selectQuery = this;
        return true;
    }
}
