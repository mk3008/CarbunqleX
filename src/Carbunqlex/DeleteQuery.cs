using Carbunqlex.Clauses;
using Carbunqlex.Lexing;
using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex;

public class DeleteQuery : IQuery
{
    public WithClause WithClause { get; }

    public DeleteClause DeleteClause { get; }

    public UsingClause? UsingClause { get; set; }

    public WhereClause WhereClause { get; set; }

    public ReturningClause? ReturningClause { get; set; }

    public Dictionary<string, object?> Parameters { get; } = new();

    public DeleteQuery(DeleteClause deleteClause)
    {
        WithClause = new WithClause();
        DeleteClause = deleteClause;
        WhereClause = new WhereClause();
        ReturningClause = null;
    }

    public DeleteQuery(DeleteClause deleteClause, WhereClause whereClause)
    {
        WithClause = new WithClause();
        DeleteClause = deleteClause;
        WhereClause = whereClause;
        ReturningClause = null;
    }

    public DeleteQuery(WithClause? withClause, DeleteClause deleteClause, UsingClause? usingClause, WhereClause? whereClause, ReturningClause? returningClause)
    {
        WithClause = withClause ?? new();
        DeleteClause = deleteClause;
        UsingClause = usingClause;
        WhereClause = whereClause ?? new();
        ReturningClause = returningClause;
    }

    public string ToSql()
    {
        var sb = new StringBuilder();

        var withSql = new WithClause(GetCommonTableClauses()).ToSql();
        if (!string.IsNullOrEmpty(withSql))
        {
            sb.Append(withSql).Append(" ");
        }

        sb.Append(DeleteClause.ToSqlWithoutCte());
        if (UsingClause != null)
        {
            sb.Append(" ").Append(UsingClause.ToSqlWithoutCte());
        }
        var whereSql = WhereClause.ToSqlWithoutCte();
        if (!string.IsNullOrEmpty(whereSql))
        {
            sb.Append(" ").Append(whereSql);
        }
        if (ReturningClause != null)
        {
            sb.Append(" ").Append(ReturningClause.ToSqlWithoutCte());
        }

        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokens()
    {
        foreach (var token in DeleteClause.GenerateTokensWithoutCte())
        {
            yield return token;
        }
        if (UsingClause != null)
        {
            foreach (var token in UsingClause.GenerateTokensWithoutCte())
            {
                yield return token;
            }
        }
        foreach (var token in WhereClause.GenerateTokensWithoutCte())
        {
            yield return token;
        }
        if (ReturningClause != null)
        {
            foreach (var token in ReturningClause.GenerateTokensWithoutCte())
            {
                yield return token;
            }
        }
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        var lst = new List<CommonTableClause>();
        lst.AddRange(WithClause.CommonTableClauses);
        lst.AddRange(DeleteClause.GetQueries().SelectMany(q => q.GetCommonTableClauses()));
        if (UsingClause != null)
        {
            lst.AddRange(UsingClause.GetQueries().SelectMany(q => q.GetCommonTableClauses()));
        }
        lst.AddRange(WhereClause.GetQueries().SelectMany(q => q.GetCommonTableClauses()));
        if (ReturningClause != null)
        {
            lst.AddRange(ReturningClause.GetQueries().SelectMany(q => q.GetCommonTableClauses()));
        }
        return lst;
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
