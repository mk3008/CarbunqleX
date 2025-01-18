using System.Text;

namespace Carbunqlex.ValueExpressions;

public class CaseExpressionWithoutCase : IValueExpression
{
    public List<WhenThenPair> WhenThenPairs { get; }
    public IValueExpression Else { get; }

    public CaseExpressionWithoutCase(List<WhenThenPair> whenThenPairs, IValueExpression elseExpression)
    {
        WhenThenPairs = whenThenPairs;
        Else = elseExpression;
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => WhenThenPairs.Any(pair => pair.When.MightHaveQueries || pair.Then.MightHaveQueries) ||
                                    Else.MightHaveQueries;

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.Keyword, "case");

        foreach (var pair in WhenThenPairs)
        {
            foreach (var lexeme in pair.GenerateTokensWithoutCte())
            {
                yield return lexeme;
            }
        }

        yield return new Token(TokenType.Keyword, "else");
        foreach (var lexeme in Else.GenerateTokensWithoutCte())
        {
            yield return lexeme;
        }

        yield return new Token(TokenType.Keyword, "end");
    }

    public string ToSqlWithoutCte()
    {
        var sql = new StringBuilder("case");

        foreach (var pair in WhenThenPairs)
        {
            sql.Append($" {pair.ToSqlWithoutCte()}");
        }

        sql.Append($" else {Else.ToSqlWithoutCte()}");
        sql.Append(" end");
        return sql.ToString();
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery>();

        foreach (var pair in WhenThenPairs)
        {
            if (pair.When.MightHaveQueries)
            {
                queries.AddRange(pair.When.GetQueries());
            }
            if (pair.Then.MightHaveQueries)
            {
                queries.AddRange(pair.Then.GetQueries());
            }
        }

        if (Else.MightHaveQueries)
        {
            queries.AddRange(Else.GetQueries());
        }

        return queries;
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        var columns = new List<ColumnExpression>();

        foreach (var pair in WhenThenPairs)
        {
            columns.AddRange(pair.ExtractColumnExpressions());
        }

        columns.AddRange(Else.ExtractColumnExpressions());

        return columns;
    }
}
