using Carbunqlex.Clauses;
using System.Text;

namespace Carbunqlex;

public class SelectQuery : IQuery
{
    public WithClause WithClause { get; } = new WithClause();
    public SelectClause SelectClause { get; }
    public IFromClause FromClause { get; set; }
    public IWhereClause WhereClause { get; set; } = EmptyWhereClause.Instance;
    public GroupByClause GroupByClause { get; set; } = new GroupByClause();
    public HavingClause HavingClause { get; set; } = new HavingClause();
    public OrderByClause OrderByClause { get; set; } = new OrderByClause();
    public WindowClause WindowClause { get; set; } = new WindowClause();
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

    /// <summary>
    /// Retrieves the common table clauses (CTEs) associated with the query.
    /// Duplicate CTEs are removed, with precedence given to those defined earlier.
    /// Recursive CTEs are prioritized, while maintaining the original order for non-recursive CTEs.
    /// </summary>
    /// <returns>The common table clauses associated with the query.</returns>
    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        var commonTableClauses = new List<(CommonTableClause Cte, int Index)>();

        commonTableClauses.AddRange(WithClause.GetCommonTableClauses().Select((cte, index) => (cte, index)));
        commonTableClauses.AddRange(SelectClause.GetCommonTableClauses().Select((cte, index) => (cte, index + commonTableClauses.Count)));
        commonTableClauses.AddRange(FromClause.GetCommonTableClauses().Select((cte, index) => (cte, index + commonTableClauses.Count)));
        commonTableClauses.AddRange(WhereClause.GetCommonTableClauses().Select((cte, index) => (cte, index + commonTableClauses.Count)));
        commonTableClauses.AddRange(GroupByClause.GetCommonTableClauses().Select((cte, index) => (cte, index + commonTableClauses.Count)));
        commonTableClauses.AddRange(HavingClause.GetCommonTableClauses().Select((cte, index) => (cte, index + commonTableClauses.Count)));
        commonTableClauses.AddRange(OrderByClause.GetCommonTableClauses().Select((cte, index) => (cte, index + commonTableClauses.Count)));
        commonTableClauses.AddRange(WindowClause.GetCommonTableClauses().Select((cte, index) => (cte, index + commonTableClauses.Count)));
        commonTableClauses.AddRange(ForClause.GetCommonTableClauses().Select((cte, index) => (cte, index + commonTableClauses.Count)));
        commonTableClauses.AddRange(PagingClause.GetCommonTableClauses().Select((cte, index) => (cte, index + commonTableClauses.Count)));

        return commonTableClauses
            .GroupBy(cte => cte.Cte.Alias)
            .Select(group => group.First())
            .OrderByDescending(cte => cte.Cte.IsRecursive)
            .ThenBy(cte => cte.Index)
            .Select(cte => cte.Cte);
    }
}
