using Carbunqlex.Clauses;
using Carbunqlex.Expressions;
using Carbunqlex.Lexing;
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
        var withSql = new WithClause(GetCommonTableClauses()).ToSql();
        if (!string.IsNullOrEmpty(withSql))
        {
            sb.Append(withSql).Append(" ");
        }

        sb.Append(UpdateClause.ToSqlWithoutCte());
        sb.Append(" ");
        sb.Append(SetClause.ToSqlWithoutCte());

        var from = FromClause.ToSqlWithoutCte();
        if (!string.IsNullOrEmpty(from))
        {
            sb.Append(" ");
            sb.Append(from);
        }
        var where = WhereClause.ToSqlWithoutCte();
        if (!string.IsNullOrEmpty(where))
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

    public IEnumerable<Token> GenerateTokens()
    {
        var cteClauses = GetCommonTableClauses().ToList();
        var tokens = new List<Token>();
        if (cteClauses.Any())
        {
            tokens.AddRange(cteClauses.SelectMany(cte => cte.GenerateTokensWithoutCte()));
        }
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
        throw new InvalidOperationException("If CTEs are omitted, the query may be incomplete. Please use the ToSql method.");
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        throw new InvalidOperationException("If CTEs are omitted, the query may be incomplete. Please use the ToSql method.");
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        yield break;
    }
}
