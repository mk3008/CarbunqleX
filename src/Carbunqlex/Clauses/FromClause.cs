using Carbunqlex.DatasourceExpressions;
using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex.Clauses;

public class FromClause : IFromClause
{
    public IDatasource RootDatasource { get; set; }

    public List<JoinClause> JoinClauses { get; } = new();

    public FromClause(IDatasource datasource)
    {
        RootDatasource = datasource;
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append($"from {RootDatasource.ToSqlWithoutCte()}");
        if (JoinClauses.Count > 0)
        {
            sb.Append(" ");
            JoinClauses.Select(j => j.ToSqlWithoutCte()).ToList().ForEach(j => sb.Append(j));
        }
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        var lexemes = new List<Lexeme> {
            new Lexeme(LexType.StartClause,"from", "from")
        };
        lexemes.AddRange(RootDatasource.GenerateLexemesWithoutCte());

        foreach (var joinClause in JoinClauses)
        {
            lexemes.AddRange(joinClause.GenerateLexemesWithoutCte());
        }

        lexemes.Add(new Lexeme(LexType.EndClause, string.Empty, "from"));

        return lexemes;
    }

    public IEnumerable<IQuery> GetQueries()
    {
        var queries = new List<IQuery>();
        queries.AddRange(RootDatasource.GetQueries());

        foreach (var joinClause in JoinClauses)
        {
            queries.AddRange(joinClause.GetQueries());
        }

        return queries;
    }

    public IEnumerable<ColumnExpression> GetSelectableColumnExpressions()
    {
        var columns = RootDatasource.GetSelectableColumnExpressions().ToList();
        foreach (var joinClause in JoinClauses)
        {
            columns.AddRange(joinClause.Datasource.GetSelectableColumnExpressions());
        }
        return columns;
    }

    public IEnumerable<IDatasource> GetDatasources()
    {
        return new IDatasource[] { RootDatasource }.Union(JoinClauses.Select(join => join.Datasource));
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        return JoinClauses
            .Where(static joinClause => joinClause.Condition != null)
            .SelectMany(static joinClause => joinClause.Condition!.ExtractColumnExpressions());
    }
}
