using System.Text;

namespace Carbunqlex.ValueExpressions;

public class CaseWhenExpression : ICaseExpression
{
    public List<WhenClause> WhenClauses { get; }
    public IValueExpression? ElseValue { get; }

    public CaseWhenExpression(List<WhenClause> whenClauses, IValueExpression elseValue)
    {
        WhenClauses = whenClauses;
        ElseValue = elseValue;
    }

    public CaseWhenExpression(List<WhenClause> whenClauses)
    {
        WhenClauses = whenClauses;
        ElseValue = null;
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => WhenClauses.Any(pair => pair.WhenValue.MightHaveQueries || pair.ThenValue.MightHaveQueries) ||
                                    (ElseValue?.MightHaveQueries ?? false);

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.Command, "case");

        foreach (var pair in WhenClauses)
        {
            foreach (var lexeme in pair.GenerateTokensWithoutCte())
            {
                yield return lexeme;
            }
        }

        if (ElseValue != null)
        {
            yield return new Token(TokenType.Command, "else");
            foreach (var lexeme in ElseValue.GenerateTokensWithoutCte())
            {
                yield return lexeme;
            }
        }

        yield return new Token(TokenType.Command, "end");
    }

    public string ToSqlWithoutCte()
    {
        var sql = new StringBuilder("case");

        foreach (var pair in WhenClauses)
        {
            sql.Append($" {pair.ToSqlWithoutCte()}");
        }

        if (ElseValue != null)
        {
            sql.Append($" else {ElseValue.ToSqlWithoutCte()}");
        }

        sql.Append(" end");
        return sql.ToString();
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery>();

        foreach (var pair in WhenClauses)
        {
            if (pair.WhenValue.MightHaveQueries)
            {
                queries.AddRange(pair.WhenValue.GetQueries());
            }
            if (pair.ThenValue.MightHaveQueries)
            {
                queries.AddRange(pair.ThenValue.GetQueries());
            }
        }

        if (ElseValue != null && ElseValue.MightHaveQueries)
        {
            queries.AddRange(ElseValue.GetQueries());
        }

        return queries;
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        var columns = new List<ColumnExpression>();

        foreach (var pair in WhenClauses)
        {
            columns.AddRange(pair.ExtractColumnExpressions());
        }

        if (ElseValue != null)
        {
            columns.AddRange(ElseValue.ExtractColumnExpressions());
        }

        return columns;
    }
}
