using Carbunqlex.DatasourceExpressions;
using System.Text;

namespace Carbunqlex.Clauses;

public class FromClause : ISqlComponent
{
    public IDatasource RootDatasource { get; set; }

    public List<JoinClause> joinClauses { get; } = new();

    public FromClause(IDatasource datasource)
    {
        RootDatasource = datasource;
    }

    public string ToSql()
    {
        var sb = new StringBuilder();
        sb.Append($"from {RootDatasource.ToSql()}");
        if (joinClauses.Count > 0)
        {
            sb.Append(" ");
            joinClauses.Select(j => j.ToSql()).ToList().ForEach(j => sb.Append(j));
        }
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        var lexemes = new List<Lexeme> {
            new Lexeme(LexType.StartClause,"from", "from")
        };
        lexemes.AddRange(RootDatasource.GetLexemes());

        foreach (var joinClause in joinClauses)
        {
            lexemes.AddRange(joinClause.GetLexemes());
        }

        lexemes.Add(new Lexeme(LexType.EndClause, string.Empty, "from"));

        return lexemes;
    }
}
