using Carbunqlex.Expressions;
using Carbunqlex.Lexing;
using System.Text;

namespace Carbunqlex.Clauses;

public class OrderByColumn : ISqlComponent
{
    public IValueExpression Column { get; }
    public bool Ascending { get; }
    public bool? NullsFirst { get; }

    public OrderByColumn(IValueExpression column, bool ascending = true, bool? nullsFirst = null)
    {
        Column = column;
        Ascending = ascending;
        NullsFirst = nullsFirst;
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(Column.ToSqlWithoutCte());
        if (!Ascending)
        {
            sb.Append(" desc");
        }
        if (NullsFirst.HasValue)
        {
            sb.Append(NullsFirst.Value ? " nulls first" : " nulls last");
        }
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>();
        tokens.AddRange(Column.GenerateTokensWithoutCte());
        if (!Ascending)
        {
            tokens.Add(new Token(TokenType.Command, "desc"));
        }
        if (NullsFirst.HasValue)
        {
            tokens.Add(new Token(TokenType.Command, NullsFirst.Value ? "nulls first" : "nulls last"));
        }
        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        if (Column.MightHaveQueries)
        {
            return Column.GetQueries();
        }
        return Enumerable.Empty<ISelectQuery>();
    }
}
