using System.Text;

namespace Carbunqlex.ValueExpressions;

public class WhenThenPair : IValueExpression
{
    public IValueExpression When { get; }
    public IValueExpression Then { get; }

    public WhenThenPair(IValueExpression when, IValueExpression then)
    {
        When = when;
        Then = then;
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => When.MightHaveQueries || Then.MightHaveQueries;

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append("when ");
        sb.Append(When.ToSqlWithoutCte());
        sb.Append(" then ");
        sb.Append(Then.ToSqlWithoutCte());
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.Keyword, "when");
        foreach (var lexeme in When.GenerateTokensWithoutCte())
        {
            yield return lexeme;
        }
        yield return new Token(TokenType.Keyword, "then");
        foreach (var lexeme in Then.GenerateTokensWithoutCte())
        {
            yield return lexeme;
        }
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery>();

        if (When.MightHaveQueries)
        {
            queries.AddRange(When.GetQueries());
        }
        if (Then.MightHaveQueries)
        {
            queries.AddRange(Then.GetQueries());
        }

        return queries;
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        var columns = new List<ColumnExpression>();

        columns.AddRange(When.ExtractColumnExpressions());
        columns.AddRange(Then.ExtractColumnExpressions());

        return columns;
    }
}
