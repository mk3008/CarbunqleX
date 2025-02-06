using Carbunqlex.Clauses;
using Carbunqlex.DatasourceExpressions;
using Carbunqlex.ValueExpressions;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Carbunqlex;

public class SelectQuery : ISelectQuery
{
    public WithClause WithClause { get; } = new WithClause();
    public SelectClause SelectClause { get; }
    public IFromClause? FromClause { get; set; }
    public WhereClause WhereClause { get; } = new WhereClause();
    public GroupByClause GroupByClause { get; } = new GroupByClause();
    public HavingClause HavingClause { get; } = new HavingClause();
    public OrderByClause OrderByClause { get; } = new OrderByClause();
    public WindowClause WindowClause { get; } = new WindowClause();
    public ILimitClause? LimitClause { get; set; }
    public OffsetClause? OffsetClause { get; set; }
    public IForClause? ForClause { get; set; }

    public SelectQuery(SelectClause selectClause)
    {
        SelectClause = selectClause;
        FromClause = EmptyFromClause.Instance;
        LimitClause = null;
        OffsetClause = null;
        ForClause = null;
    }

    public SelectQuery(SelectClause selectClause, IFromClause fromClause)
    {
        SelectClause = selectClause;
        FromClause = fromClause;
        LimitClause = null;
        OffsetClause = null;
        ForClause = null;
    }

    public string ToSql()
    {
        var sb = new StringBuilder();

        var withSql = new WithClause(GetCommonTableClauses()).ToSql();
        if (!string.IsNullOrEmpty(withSql))
        {
            sb.Append(withSql).Append(" ");
        }

        sb.Append(ToSqlWithoutCte());
        return sb.ToString();
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();

        sb.Append(SelectClause.ToSqlWithoutCte());

        if (FromClause != null)
        {
            var fromSql = FromClause.ToSqlWithoutCte();
            if (!string.IsNullOrEmpty(fromSql))
            {
                sb.Append(" ").Append(fromSql);
            }
        }

        var whereSql = WhereClause.ToSqlWithoutCte();
        if (!string.IsNullOrEmpty(whereSql))
        {
            sb.Append(" ").Append(whereSql);
        }

        var groupBySql = GroupByClause.ToSqlWithoutCte();
        if (!string.IsNullOrEmpty(groupBySql))
        {
            sb.Append(" ").Append(groupBySql);
        }

        var havingSql = HavingClause.ToSqlWithoutCte();
        if (!string.IsNullOrEmpty(havingSql))
        {
            sb.Append(" ").Append(havingSql);
        }

        var orderBySql = OrderByClause.ToSqlWithoutCte();
        if (!string.IsNullOrEmpty(orderBySql))
        {
            sb.Append(" ").Append(orderBySql);
        }

        var windowSql = WindowClause.ToSqlWithoutCte();
        if (!string.IsNullOrEmpty(windowSql))
        {
            sb.Append(" ").Append(windowSql);
        }

        if (LimitClause != null)
        {
            var limitSql = LimitClause.ToSqlWithoutCte();
            if (!string.IsNullOrEmpty(limitSql))
            {
                sb.Append(" ").Append(limitSql);
            }
        }

        if (OffsetClause != null)
        {
            var offsetSql = OffsetClause.ToSqlWithoutCte();
            if (!string.IsNullOrEmpty(offsetSql))
            {
                sb.Append(" ").Append(offsetSql);
            }
        }

        if (ForClause != null)
        {
            var forSql = ForClause.ToSqlWithoutCte();
            if (!string.IsNullOrEmpty(forSql))
            {
                sb.Append(" ").Append(forSql);
            }
        }

        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokens()
    {
        var tokens = new List<Token>();

        tokens.AddRange(WithClause.GenerateTokensWithoutCte());
        tokens.AddRange(GenerateTokensWithoutCte());

        return tokens;
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>();

        tokens.AddRange(SelectClause.GenerateTokensWithoutCte());
        if (FromClause != null) tokens.AddRange(FromClause.GenerateTokensWithoutCte());
        tokens.AddRange(WhereClause.GenerateTokensWithoutCte());
        tokens.AddRange(GroupByClause.GenerateTokensWithoutCte());
        tokens.AddRange(HavingClause.GenerateTokensWithoutCte());
        tokens.AddRange(OrderByClause.GenerateTokensWithoutCte());
        tokens.AddRange(WindowClause.GenerateTokensWithoutCte());
        if (LimitClause != null) tokens.AddRange(LimitClause.GenerateTokensWithoutCte());
        if (OffsetClause != null) tokens.AddRange(OffsetClause.GenerateTokensWithoutCte());
        if (ForClause != null) tokens.AddRange(ForClause.GenerateTokensWithoutCte());

        return tokens;
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        var commonTables = new List<CommonTableClause>();

        // Prioritize internal CTEs
        var queries = GetQueries().Where(q => q != this).ToList();
        foreach (var q in queries)
        {
            commonTables.AddRange(q.GetCommonTableClauses());
        }
        commonTables.AddRange(WithClause.CommonTableClauses);

        return commonTables;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery>();

        queries.Add(this);
        queries.AddRange(WithClause.GetQueries());
        queries.AddRange(SelectClause.GetQueries());
        if (FromClause != null) queries.AddRange(FromClause.GetQueries());
        queries.AddRange(WhereClause.GetQueries());
        queries.AddRange(GroupByClause.GetQueries());
        queries.AddRange(HavingClause.GetQueries());
        queries.AddRange(OrderByClause.GetQueries());
        queries.AddRange(WindowClause.GetQueries());
        if (LimitClause != null) queries.AddRange(LimitClause.GetQueries());
        if (OffsetClause != null) queries.AddRange(OffsetClause.GetQueries());
        if (ForClause != null) queries.AddRange(ForClause.GetQueries());

        return queries;
    }

    public Dictionary<string, object?> Parameters { get; } = new();
    public bool MightHaveQueries => true;

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
        return SelectClause.Expressions;
    }

    public IEnumerable<DatasourceExpression> GetDatasources()
    {
        if (FromClause == null)
        {
            return Enumerable.Empty<DatasourceExpression>();
        }
        return FromClause.GetDatasources();
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        var columnExpressions = new List<ColumnExpression>();

        columnExpressions.AddRange(SelectClause.ExtractColumnExpressions());
        if (FromClause != null) columnExpressions.AddRange(FromClause.ExtractColumnExpressions());
        columnExpressions.AddRange(WhereClause.ExtractColumnExpressions());

        return columnExpressions;
    }

    public bool TryGetWhereClause([NotNullWhen(true)] out WhereClause? whereClause)
    {
        whereClause = WhereClause;
        return true;
    }

    public ParameterExpression AddParameter(string name, object value)
    {
        var parameter = new ParameterExpression(name);
        Parameters[name] = value;
        return parameter;
    }

    public void AddColumn(SelectExpression expr)
    {
        SelectClause.Expressions.Add(expr);
    }

    public void AddColumn(IValueExpression value, string alias)
    {
        SelectClause.Expressions.Add(new SelectExpression(value, alias));
    }

    public void RemoveColumn(SelectExpression expr)
    {
        SelectClause.Expressions.Remove(expr);
    }

    public void AddJoin(JoinClause joinClause)
    {
        if (FromClause == null)
        {
            throw new InvalidOperationException("Cannot add a join clause without a FROM clause.");
        }
        FromClause.AddJoin(joinClause);
    }
}
