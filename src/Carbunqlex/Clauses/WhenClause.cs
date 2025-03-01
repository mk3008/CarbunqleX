using Carbunqlex.Expressions;
using Carbunqlex.Lexing;
using System.Text;

namespace Carbunqlex.Clauses;

public class WhenClause : IValueExpression
{
    public IValueExpression WhenValue { get; }
    public IValueExpression ThenValue { get; }

    public WhenClause(IValueExpression when, IValueExpression then)
    {
        WhenValue = when;
        ThenValue = then;
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => WhenValue.MightHaveQueries || ThenValue.MightHaveQueries;

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append("when ");
        sb.Append(WhenValue.ToSqlWithoutCte());
        sb.Append(" then ");
        sb.Append(ThenValue.ToSqlWithoutCte());
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.Command, "when");
        foreach (var lexeme in WhenValue.GenerateTokensWithoutCte())
        {
            yield return lexeme;
        }
        yield return new Token(TokenType.Command, "then");
        foreach (var lexeme in ThenValue.GenerateTokensWithoutCte())
        {
            yield return lexeme;
        }
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery>();

        if (WhenValue.MightHaveQueries)
        {
            queries.AddRange(WhenValue.GetQueries());
        }
        if (ThenValue.MightHaveQueries)
        {
            queries.AddRange(ThenValue.GetQueries());
        }

        return queries;
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        var columns = new List<ColumnExpression>();

        columns.AddRange(WhenValue.ExtractColumnExpressions());
        columns.AddRange(ThenValue.ExtractColumnExpressions());

        return columns;
    }
}
