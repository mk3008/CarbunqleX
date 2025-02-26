using Carbunqlex.Lexing;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Carbunqlex.QuerySources;

public class SubQuerySource : IDatasource
{
    public ISelectQuery Query { get; set; }

    public string TableFullName => string.Empty;

    public string DefaultName => string.Empty;

    public SubQuerySource(ISelectQuery query)
    {
        Query = query;
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append("(");
        sb.Append(Query.ToSqlWithoutCte());
        sb.Append(")");
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>
        {
            new Token(TokenType.OpenParen, "(")
        };
        tokens.AddRange(Query.GenerateTokensWithoutCte());
        tokens.Add(new Token(TokenType.CloseParen, ")"));
        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery> { Query };
        queries.AddRange(Query.GetQueries());
        return queries;
    }

    public IEnumerable<string> GetSelectableColumns()
    {
        return Query.GetSelectExpressions().Select(column => column.Alias);
    }

    public bool TryGetSubQuery([NotNullWhen(true)] out ISelectQuery subQuery)
    {
        subQuery = Query;
        return true;
    }

    public bool TryGetTableName([NotNullWhen(true)] out string? tableFullName)
    {
        tableFullName = null;
        return false;
    }

    public bool TryGetUnionQuerySource([NotNullWhen(true)] out UnionQuerySource? unionQuerySource)
    {
        unionQuerySource = null;
        return false;
    }
}
