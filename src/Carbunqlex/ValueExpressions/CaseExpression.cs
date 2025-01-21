using System.Text;

namespace Carbunqlex.ValueExpressions;

public class CaseExpression : ICaseExpression
{
    public IValueExpression CaseValue { get; }
    public List<WhenClause> WhenClauses { get; }
    public IValueExpression? ElseValue { get; }

    public CaseExpression(IValueExpression caseValue, List<WhenClause> whenClauses, IValueExpression elseValue)
    {
        CaseValue = caseValue;
        WhenClauses = whenClauses;
        ElseValue = elseValue;
    }

    public CaseExpression(IValueExpression caseValue, List<WhenClause> whenClauses)
    {
        CaseValue = caseValue;
        WhenClauses = whenClauses;
        ElseValue = null;
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => CaseValue.MightHaveQueries ||
                                    WhenClauses.Any(pair => pair.WhenValue.MightHaveQueries || pair.ThenValue.MightHaveQueries) ||
                                    (ElseValue?.MightHaveQueries ?? false);

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.Command, "case");

        foreach (var lexeme in CaseValue.GenerateTokensWithoutCte())
        {
            yield return lexeme;
        }

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
        sql.Append($" {CaseValue.ToSqlWithoutCte()}");

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

        if (CaseValue.MightHaveQueries)
        {
            queries.AddRange(CaseValue.GetQueries());
        }

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

        columns.AddRange(CaseValue.ExtractColumnExpressions());

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
