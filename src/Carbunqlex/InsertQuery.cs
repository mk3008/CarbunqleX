using Carbunqlex.Clauses;
using Carbunqlex.Expressions;
using Carbunqlex.Lexing;
using System.Text;

namespace Carbunqlex;

public class InsertQuery : IQuery
{
    public InsertClause InsertClause { get; set; }

    public ISelectQuery SelectQuery { get; set; }

    public Dictionary<string, object?> Parameters { get; } = new();

    public ReturningClause? Returning { get; set; }

    public InsertQuery(InsertClause insertClause, ISelectQuery selectQuery, ReturningClause? returning = null)
    {
        InsertClause = insertClause;
        SelectQuery = selectQuery;
        Returning = returning;
    }

    public string ToSqlWithoutCte()
    {
        throw new InvalidOperationException("If CTEs are omitted, the query may be incomplete. Please use the ToSql method.");
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        throw new InvalidOperationException("If CTEs are omitted, the query may be incomplete. Please use the ToSql method.");
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        yield return SelectQuery;
    }

    public string ToSql()
    {
        var sb = new StringBuilder(InsertClause.ToSqlWithoutCte());

        // with clause
        sb.Append(" ");
        var withSql = new WithClause(GetCommonTableClauses()).ToSql();
        if (!string.IsNullOrEmpty(withSql))
        {
            sb.Append(withSql).Append(" ");
        }

        // other clauses
        sb.Append(SelectQuery.ToSqlWithoutCte());
        if (Returning != null)
        {
            sb.Append($" {Returning.ToSqlWithoutCte()}");
        }
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokens()
    {
        var tokens = new List<Token>();
        tokens.AddRange(InsertClause.GenerateTokensWithoutCte());

        var cteClauses = GetCommonTableClauses().ToList();
        if (cteClauses.Any())
        {
            tokens.Add(new Token(TokenType.Command, "with"));
            foreach (var cte in cteClauses)
            {
                tokens.AddRange(cte.GenerateTokensWithoutCte());
            }
        }
        tokens.AddRange(SelectQuery.GenerateTokensWithoutCte());

        if (Returning != null)
        {
            tokens.AddRange(Returning.GenerateTokensWithoutCte());
        }

        return tokens;
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        return SelectQuery.GetCommonTableClauses();
    }

    public IDictionary<string, object?> GetParameters()
    {
        return Parameters;
    }

    public ParameterExpression AddParameter(string name, object value)
    {
        var parameter = new ParameterExpression(name);
        Parameters[name] = value;
        return parameter;
    }
}
