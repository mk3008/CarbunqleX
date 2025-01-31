using Carbunqlex.DatasourceExpressions;
using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex.Clauses;

public class JoinClause : ISqlComponent
{
    public DatasourceExpression Datasource { get; set; }
    public string JoinKeyword { get; set; }
    public IValueExpression? Condition { get; set; }
    public bool IsLateral { get; set; }

    public JoinClause(DatasourceExpression datasource, string joinKeyword, IValueExpression condition)
    {
        Datasource = datasource;
        JoinKeyword = joinKeyword;
        Condition = condition;
        IsLateral = false;
    }

    public JoinClause(DatasourceExpression datasource, string joinKeyword)
    {
        Datasource = datasource;
        JoinKeyword = joinKeyword;
        IsLateral = false;
    }

    private string GetClauseText()
    {
        var sb = new StringBuilder();
        sb.Append(JoinKeyword.ToLower());
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

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var clause = GetClauseText();

        var tokens = new List<Token>
        {
            new Token(TokenType.StartClause, clause, "join")
        };

        tokens.AddRange(Datasource.GenerateTokensWithoutCte());

        if (Condition != null)
        {
            tokens.Add(new Token(TokenType.Command, "on"));
            tokens.AddRange(Condition.GenerateTokensWithoutCte());
        }

        tokens.Add(new Token(TokenType.EndClause, string.Empty, "join"));

        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery>();
        queries.AddRange(Datasource.GetQueries());

        if (Condition != null)
        {
            queries.AddRange(Condition.GetQueries());
        }

        return queries;
    }
}
