using Carbunqlex.DatasourceExpressions;
using System.Text;

namespace Carbunqlex.Clauses;

public class FromClause : IFromClause
{
    public IDatasource RootDatasource { get; set; }

    public List<JoinClause> joinClauses { get; } = new();

    public FromClause(IDatasource datasource)
    {
        RootDatasource = datasource;
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append($"from {RootDatasource.ToSqlWithoutCte()}");
        if (joinClauses.Count > 0)
        {
            sb.Append(" ");
            joinClauses.Select(j => j.ToSqlWithoutCte()).ToList().ForEach(j => sb.Append(j));
        }
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        var lexemes = new List<Lexeme> {
            new Lexeme(LexType.StartClause,"from", "from")
        };
        lexemes.AddRange(RootDatasource.GenerateLexemesWithoutCte());

        foreach (var joinClause in joinClauses)
        {
            lexemes.AddRange(joinClause.GenerateLexemesWithoutCte());
        }

        lexemes.Add(new Lexeme(LexType.EndClause, string.Empty, "from"));

        return lexemes;
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        var commonTableClauses = new List<CommonTableClause>();
        commonTableClauses.AddRange(RootDatasource.GetCommonTableClauses());

        foreach (var joinClause in joinClauses)
        {
            commonTableClauses.AddRange(joinClause.GetCommonTableClauses());
        }

        return commonTableClauses;
    }
}
