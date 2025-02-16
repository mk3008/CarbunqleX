using Carbunqlex.Clauses;
using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex;

public class UpdateQuery : IQuery
{
    public WithClause WithClause { get; set; }
    public UpdateClause UpdateClause { get; set; }
    public SetClause SetClause { get; set; }
    public IFromClause FromClause { get; set; }
    public WhereClause WhereClause { get; set; }
    public Dictionary<string, object?> Parameters { get; } = new();

    public ReturningClause? Returning { get; set; }

    public UpdateQuery(WithClause? withClause, UpdateClause updateClause, SetClause setClause, IFromClause fromClause, WhereClause whereClause, ReturningClause? returning = null)
    {
        WithClause = withClause ?? new WithClause();
        UpdateClause = updateClause;
        SetClause = setClause;
        FromClause = fromClause;
        WhereClause = whereClause;
        Returning = returning;
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
        foreach (var cte in WithClause.CommonTableClauses)
        {
            yield return cte;
        }
        foreach (var cte in FromClause.GetQueries().SelectMany(x => x.GetCommonTableClauses()))
        {
            yield return cte;
        }
        foreach (var cte in WhereClause.GetQueries().SelectMany(x => x.GetCommonTableClauses()))
        {
            yield return cte;
        }
    }

    public IDictionary<string, object?> GetParameters()
    {
        return Parameters;
    }

    public ParameterExpression AddParameter(string name, object value)
    {
        Parameters[name] = value;
        return new ParameterExpression(name);
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(UpdateClause.ToSqlWithoutCte());
        sb.Append(" ");
        sb.Append(SetClause.ToSqlWithoutCte());

        var from = FromClause.ToSqlWithoutCte();
        if (string.IsNullOrEmpty(from) == false)
        {
            sb.Append(" ");
            sb.Append(from);
        }
        var where = WhereClause.ToSqlWithoutCte();
        if (string.IsNullOrEmpty(where) == false)
        {
            sb.Append(" ");
            sb.Append(where);
        }
        if (Returning != null)
        {
            sb.Append(" ");
            sb.Append(Returning.ToSqlWithoutCte());
        }
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>();
        tokens.AddRange(UpdateClause.GenerateTokensWithoutCte());
        tokens.AddRange(SetClause.GenerateTokensWithoutCte());
        tokens.AddRange(FromClause.GenerateTokensWithoutCte());
        tokens.AddRange(WhereClause.GenerateTokensWithoutCte());
        if (Returning != null)
        {
            tokens.AddRange(Returning.GenerateTokensWithoutCte());
        }
        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        yield break;
    }
}
