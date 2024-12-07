using Carbunqlex.Clauses;
using System.Text;

namespace Carbunqlex;

public class SelectQuery : IQuery
{
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

        sb.Append(SelectClause.ToSql());

        var fromSql = FromClause.ToSql();
        if (!string.IsNullOrEmpty(fromSql))
        {
            sb.Append(" ").Append(fromSql);
        }

        var whereSql = WhereClause.ToSql();
        if (!string.IsNullOrEmpty(whereSql))
        {
            sb.Append(" ").Append(whereSql);
        }

        var groupBySql = GroupByClause.ToSql();
        if (!string.IsNullOrEmpty(groupBySql))
        {
            sb.Append(" ").Append(groupBySql);
        }

        var havingSql = HavingClause.ToSql();
        if (!string.IsNullOrEmpty(havingSql))
        {
            sb.Append(" ").Append(havingSql);
        }

        var orderBySql = OrderByClause.ToSql();
        if (!string.IsNullOrEmpty(orderBySql))
        {
            sb.Append(" ").Append(orderBySql);
        }

        var windowSql = WindowClause.ToSql();
        if (!string.IsNullOrEmpty(windowSql))
        {
            sb.Append(" ").Append(windowSql);
        }

        var forSql = ForClause.ToSql();
        if (!string.IsNullOrEmpty(forSql))
        {
            sb.Append(" ").Append(forSql);
        }

        var pagingSql = PagingClause.ToSql();
        if (!string.IsNullOrEmpty(pagingSql))
        {
            sb.Append(" ").Append(pagingSql);
        }

        return sb.ToString();
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        var lexemes = new List<Lexeme>();

        lexemes.AddRange(SelectClause.GetLexemes());
        lexemes.AddRange(FromClause.GetLexemes());
        lexemes.AddRange(WhereClause.GetLexemes());
        lexemes.AddRange(GroupByClause.GetLexemes());
        lexemes.AddRange(HavingClause.GetLexemes());
        lexemes.AddRange(OrderByClause.GetLexemes());
        lexemes.AddRange(WindowClause.GetLexemes());
        lexemes.AddRange(ForClause.GetLexemes());
        lexemes.AddRange(PagingClause.GetLexemes());

        return lexemes;
    }
}
