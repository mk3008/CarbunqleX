using Carbunqlex.DatasourceExpressions;
using Carbunqlex.Lexing;
using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex.Clauses;

public class FromClause : IFromClause
{
    public DatasourceExpression RootDatasource { get; set; }

    public List<JoinClause> JoinClauses { get; }

    public FromClause(DatasourceExpression datasource)
    {
        RootDatasource = datasource;
        JoinClauses = new List<JoinClause>();
    }

    public FromClause(DatasourceExpression datasource, List<JoinClause> joinClauses)
    {
        RootDatasource = datasource;
        JoinClauses = joinClauses;
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append($"from {RootDatasource.ToSqlWithoutCte()}");
        if (JoinClauses.Count > 0)
        {
            JoinClauses.Select(j => j.ToSqlWithoutCte()).ToList().ForEach(j => sb.Append(" ").Append(j));
        }
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token> {
            new Token(TokenType.StartClause,"from", "from")
        };
        tokens.AddRange(RootDatasource.GenerateTokensWithoutCte());

        foreach (var joinClause in JoinClauses)
        {
            tokens.AddRange(joinClause.GenerateTokensWithoutCte());
        }

        tokens.Add(new Token(TokenType.EndClause, string.Empty, "from"));

        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery>();
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

    public IEnumerable<DatasourceExpression> GetDatasources()
    {
        return new DatasourceExpression[] { RootDatasource }.Union(JoinClauses.Select(join => join.Datasource));
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        return JoinClauses
            .Where(static joinClause => joinClause.Condition != null)
            .SelectMany(static joinClause => joinClause.Condition!.ExtractColumnExpressions());
    }

    public void AddJoin(JoinClause joinClause)
    {
        JoinClauses.Add(joinClause);
    }
}
