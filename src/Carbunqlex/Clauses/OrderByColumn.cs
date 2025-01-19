using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex.Clauses;

public class OrderByColumn : ISqlComponent
{
    public IValueExpression Column { get; }
    public bool Ascending { get; }

    public OrderByColumn(IValueExpression column, bool ascending = true)
    {
        Column = column;
        Ascending = ascending;
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(Column.ToSqlWithoutCte());
        if (!Ascending)
        {
            sb.Append(" desc");
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
