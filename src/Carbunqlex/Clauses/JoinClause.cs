using Carbunqlex.DatasourceExpressions;
using Carbunqlex.QueryModels;
using System.Text;

namespace Carbunqlex.Clauses;

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

    public string ToSql()
    {
        var joinTypeString = GetClauseText();

        if (Condition == null)
        {
            return $" {joinTypeString} {Datasource.ToSql()}";
        }

        var conditionString = Condition?.ToSql() ?? string.Empty;
        return $" {joinTypeString} {Datasource.ToSql()} on {conditionString}";
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        var clause = GetClauseText();

        var lexemes = new List<Lexeme>
        {
            new Lexeme(LexType.StartClause, clause, "join")
        };

        lexemes.AddRange(Datasource.GetLexemes());

        if (Condition != null)
        {
            lexemes.Add(new Lexeme(LexType.Keyword, "on"));
            lexemes.AddRange(Condition.GetLexemes());
        }

        new Lexeme(LexType.EndClause, string.Empty, "join");

        return lexemes;
    }
}

public enum JoinType
{
    Inner,
    Left,
    Right,
    Full,
    Cross,
    Natural
}
