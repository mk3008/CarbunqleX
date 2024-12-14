using Carbunqlex.DatasourceExpressions;
using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex.Clauses;

public enum JoinType
{
    Inner,
    Left,
    Right,
    Full,
    Cross,
    Natural
}

public class JoinClause : ISqlComponent
{
    public IDatasource Datasource { get; set; }
    public JoinType JoinType { get; set; }
    public IValueExpression? Condition { get; set; }
    public bool IsLateral { get; set; }

    public JoinClause(IDatasource datasource, JoinType joinType, IValueExpression condition)
    {
        Datasource = datasource;
        JoinType = joinType;
        Condition = condition;
        IsLateral = false;
    }

    public JoinClause(IDatasource datasource, JoinType joinType)
    {
        Datasource = datasource;
        JoinType = joinType;
        IsLateral = false;
    }

    private string GetClauseText()
    {
        var sb = new StringBuilder();
        sb.Append(JoinType.ToString().ToLower());
        sb.Append(" join");
        if (IsLateral)
        {
            sb.Append(" lateral");
        }
        return sb.ToString();
    }

    public string ToSqlWithoutCte()
    {
        var joinTypeString = GetClauseText();

        if (Condition == null)
        {
            return $"{joinTypeString} {Datasource.ToSqlWithoutCte()}";
        }

        var conditionString = Condition?.ToSqlWithoutCte() ?? string.Empty;
        return $"{joinTypeString} {Datasource.ToSqlWithoutCte()} on {conditionString}";
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        var clause = GetClauseText();

        var lexemes = new List<Lexeme>
        {
            new Lexeme(LexType.StartClause, clause, "join")
        };

        lexemes.AddRange(Datasource.GenerateLexemesWithoutCte());

        if (Condition != null)
        {
            lexemes.Add(new Lexeme(LexType.Keyword, "on"));
            lexemes.AddRange(Condition.GenerateLexemesWithoutCte());
        }

        lexemes.Add(new Lexeme(LexType.EndClause, string.Empty, "join"));

        return lexemes;
    }

    public IEnumerable<IQuery> GetQueries()
    {
        var queries = new List<IQuery>();
        queries.AddRange(Datasource.GetQueries());

        if (Condition != null)
        {
            queries.AddRange(Condition.GetQueries());
        }

        return queries;
    }
}
