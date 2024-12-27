using Carbunqlex.Clauses;
using Carbunqlex.DatasourceExpressions;
using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex;

public class SelectQuery : IQuery
{
    public WithClause WithClause { get; } = new WithClause();
    public SelectClause SelectClause { get; }
    public IFromClause FromClause { get; set; }
    public IWhereClause WhereClause { get; set; } = EmptyWhereClause.Instance;
    public GroupByClause GroupByClause { get; } = new GroupByClause();
    public HavingClause HavingClause { get; } = new HavingClause();
    public OrderByClause OrderByClause { get; } = new OrderByClause();
    public WindowClause WindowClause { get; } = new WindowClause();
    public IForClause ForClause { get; set; } = EmptyForClause.Instance;
    public IPagingClause PagingClause { get; set; } = EmptyPagingClause.Instance;

    public SelectQuery(SelectClause selectClause)
    {
        SelectClause = selectClause;
        FromClause = EmptyFromClause.Instance;
    }

    public SelectQuery(SelectClause selectClause, IFromClause fromClause)
    {
        SelectClause = selectClause;
        FromClause = fromClause;
    }

    public string ToSql()
    {
        var sb = new StringBuilder();

        // Generate the SQL for the common table expressions (CTEs) associated with the query.
        // For example, if WITH clauses are used in subqueries or inline queries, retrieve and centralize those CTEs.
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

        var fromSql = FromClause.ToSqlWithoutCte();
        if (!string.IsNullOrEmpty(fromSql))
        {
            sb.Append(" ").Append(fromSql);
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

        var forSql = ForClause.ToSqlWithoutCte();
        if (!string.IsNullOrEmpty(forSql))
        {
            sb.Append(" ").Append(forSql);
        }

        var pagingSql = PagingClause.ToSqlWithoutCte();
        if (!string.IsNullOrEmpty(pagingSql))
        {
            sb.Append(" ").Append(pagingSql);
        }

        return sb.ToString();
    }

    public IEnumerable<Lexeme> GenerateLexemes()
    {
        var lexemes = new List<Lexeme>();

        lexemes.AddRange(WithClause.GenerateLexemes());
        lexemes.AddRange(GenerateLexemesWithoutCte());

        return lexemes;
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        var lexemes = new List<Lexeme>();

        lexemes.AddRange(SelectClause.GenerateLexemesWithoutCte());
        lexemes.AddRange(FromClause.GenerateLexemesWithoutCte());
        lexemes.AddRange(WhereClause.GenerateLexemesWithoutCte());
        lexemes.AddRange(GroupByClause.GenerateLexemesWithoutCte());
        lexemes.AddRange(HavingClause.GenerateLexemesWithoutCte());
        lexemes.AddRange(OrderByClause.GenerateLexemesWithoutCte());
        lexemes.AddRange(WindowClause.GenerateLexemesWithoutCte());
        lexemes.AddRange(ForClause.GenerateLexemesWithoutCte());
        lexemes.AddRange(PagingClause.GenerateLexemesWithoutCte());

        return lexemes;
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

    public IEnumerable<IQuery> GetQueries()
    {
        var queries = new List<IQuery>();

        queries.Add(this);
        queries.AddRange(WithClause.GetQueries());
        queries.AddRange(SelectClause.GetQueries());
        queries.AddRange(FromClause.GetQueries());
        queries.AddRange(WhereClause.GetQueries());
        queries.AddRange(GroupByClause.GetQueries());
        queries.AddRange(HavingClause.GetQueries());
        queries.AddRange(OrderByClause.GetQueries());
        queries.AddRange(WindowClause.GetQueries());
        queries.AddRange(ForClause.GetQueries());
        queries.AddRange(PagingClause.GetQueries());

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
        return SelectClause.Expressions;
    }

    public IEnumerable<IDatasource> GetDatasources()
    {
        return FromClause.GetDatasources();
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        var columnExpressions = new List<ColumnExpression>();

        columnExpressions.AddRange(SelectClause.ExtractColumnExpressions());
        columnExpressions.AddRange(FromClause.ExtractColumnExpressions());
        columnExpressions.AddRange(WhereClause.ExtractColumnExpressions());

        return columnExpressions;
    }
}
