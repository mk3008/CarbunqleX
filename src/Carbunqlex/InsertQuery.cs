using Carbunqlex.Clauses;
using Carbunqlex.ValueExpressions;
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
        var sb = new StringBuilder($"{InsertClause.ToSqlWithoutCte()} {SelectQuery.ToSqlWithoutCte()}");
        if (Returning != null)
        {
            sb.Append($" {Returning.ToSqlWithoutCte()}");
        }
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        foreach (var token in InsertClause.GenerateTokensWithoutCte())
        {
            yield return token;
        }
        foreach (var token in SelectQuery.GenerateTokensWithoutCte())
        {
            yield return token;
        }
        if (Returning != null)
        {
            foreach (var token in Returning.GenerateTokensWithoutCte())
            {
                yield return token;
            }
        }
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        yield return SelectQuery;
    }

    public string ToSql()
    {
        var sb = new StringBuilder();
        var cteClauses = GetCommonTableClauses().ToList();
        if (cteClauses.Any())
        {
            sb.Append("with ");
            sb.Append(string.Join(", ", cteClauses.Select(cte => cte.ToSqlWithoutCte())));
            sb.Append(" ");
        }
        sb.Append(ToSqlWithoutCte());
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokens()
    {
        var cteClauses = GetCommonTableClauses().ToList();
        if (cteClauses.Any())
        {
            yield return new Token(TokenType.Command, "with");
            foreach (var cte in cteClauses)
            {
                foreach (var token in cte.GenerateTokensWithoutCte())
                {
                    yield return token;
                }
            }
        }
        foreach (var token in GenerateTokensWithoutCte())
        {
            yield return token;
        }
        if (Returning != null)
        {
            foreach (var token in Returning.GenerateTokensWithoutCte())
            {
                yield return token;
            }
        }
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
