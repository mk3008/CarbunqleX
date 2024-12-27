using Carbunqlex.Clauses;
using System.Text;

namespace Carbunqlex.ValueExpressions;

public class FunctionExpression : IValueExpression
{
    public List<IValueExpression> Arguments { get; set; }
    public string FunctionName { get; set; }
    public IOverClause OverClause { get; set; }

    public FunctionExpression(string functionName, params IValueExpression[] arguments)
    {
        FunctionName = functionName;
        Arguments = arguments.ToList();
        OverClause = EmptyOverClause.Instance;
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(FunctionName);
        sb.Append("(");
        sb.Append(string.Join(", ", Arguments.Select(arg => arg.ToSqlWithoutCte())));
        sb.Append(")");

        var overClause = OverClause.ToSqlWithoutCte();
        if (!string.IsNullOrEmpty(overClause))
        {
            sb.Append(" ").Append(overClause);
        }
        return sb.ToString();
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => Arguments.Any(arg => arg.MightHaveQueries) || OverClause.MightHaveCommonTableClauses;

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        yield return new Lexeme(LexType.Keyword, FunctionName);
        yield return new Lexeme(LexType.OpenParen, "(");
        for (int i = 0; i < Arguments.Count; i++)
        {
            foreach (var lexeme in Arguments[i].GenerateLexemesWithoutCte())
            {
                yield return lexeme;
            }
            if (i < Arguments.Count - 1)
            {
                yield return Lexeme.Comma;
            }
        }
        yield return new Lexeme(LexType.CloseParen, ")");
        foreach (var lexeme in OverClause.GenerateLexemesWithoutCte())
        {
            yield return lexeme;
        }
    }

    public IEnumerable<IQuery> GetQueries()
    {
        var queries = new List<IQuery>();

        foreach (var argument in Arguments)
        {
            if (argument.MightHaveQueries)
            {
                queries.AddRange(argument.GetQueries());
            }
        }

        if (OverClause.MightHaveCommonTableClauses)
        {
            queries.AddRange(OverClause.GetQueries());
        }

        return queries;
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        return Arguments.SelectMany(arg => arg.ExtractColumnExpressions());
    }
}
