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
        var withSql = WithClause.ToSql();
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
        var commonTableClauses = new List<CommonTableClause>();
        commonTableClauses.AddRange(WithClause.GetCommonTableClauses());
        commonTableClauses.AddRange(SelectClause.GetCommonTableClauses());
        commonTableClauses.AddRange(FromClause.GetCommonTableClauses());
        commonTableClauses.AddRange(WhereClause.GetCommonTableClauses());
        commonTableClauses.AddRange(GroupByClause.GetCommonTableClauses());
        commonTableClauses.AddRange(HavingClause.GetCommonTableClauses());
        commonTableClauses.AddRange(OrderByClause.GetCommonTableClauses());
        commonTableClauses.AddRange(WindowClause.GetCommonTableClauses());
        commonTableClauses.AddRange(ForClause.GetCommonTableClauses());
        commonTableClauses.AddRange(PagingClause.GetCommonTableClauses());
        return commonTableClauses;
    }
}
